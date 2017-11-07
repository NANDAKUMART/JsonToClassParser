using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using xx;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Runtime;
using System.Runtime.Caching;
using xx1;
using System.Threading;

namespace JsonParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputText = File.ReadAllText("TextFile1.txt");
            var st = DateTime.Now;
            var obj = JsonConvert.DeserializeObject<Text1Root>(inputText);
            var end = DateTime.Now;
            Console.WriteLine("NewtonJsoft: " + (end - st).Milliseconds);

            st = DateTime.Now;
            obj = ConvertJsonStrToClassStructure<Text1Root>(inputText);
            end = DateTime.Now;
            Console.WriteLine("Custom: " + (end - st).Milliseconds);

            Console.ReadLine();
        }

        public class PropsInfo
        {
            public string className;
            public string propName;
            public string propType;
            public bool isCollection;
        }

        public static void BuildPropDic(Type type, List<PropsInfo> propInfoList)
        {
            //var targetType = typeof(T);
            var propList = type.GetProperties();

            foreach (var prop in propList)
            {
                var propName = prop.Name;
                var propType = prop.PropertyType;
                var propTypeName = propType.Name;
                var isCollection = propType.IsGenericType;

                if (propType.IsGenericType) //&& propType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    propTypeName = propType.GetGenericArguments()[0].Name;
                    if (!propInfoList.Any(x => x.className == propTypeName))
                    {
                        var runTimeType = propType.GetGenericArguments()[0];
                        BuildPropDic(runTimeType, propInfoList);
                    }
                }
                else if (propType.IsClass && !propType.FullName.StartsWith("System."))
                {
                    if (!propInfoList.Any(x => x.className == propTypeName))
                    {
                        BuildPropDic(propType, propInfoList);
                    }
                }

                propInfoList.Add(new PropsInfo() { isCollection = isCollection, className = type.Name, propName = propName, propType = propTypeName });
            }
        }

        public static T ConvertJsonStrToClassStructure<T>(string jsonInput)
        {
            var tgtClsName = typeof(T).Name;
            var tgtNamespace = typeof(T).Namespace;

            var txt = new StringBuilder().Append("\"" + tgtClsName + "\":").Append(jsonInput.Trim().Replace(" ", "").Replace("\n", "").Replace("\t", "").Replace("\r", "")).ToString();

            var propInfoList = new List<PropsInfo>();
            BuildPropDic(typeof(T), propInfoList);

            var paranthesisTree = constructParanthesisTree(txt, txt.IndexOf('{'), '}');
            var entireClass = ConstructEntireClassSnippet(txt, paranthesisTree, false, 0, propInfoList);

            var prefix = @"using System;
            using System.Collections.Generic;
            using Microsoft.CSharp;
            using System.CodeDom.Compiler;
            using System.Reflection;
            using System.Linq; 
            using " + tgtNamespace + ";";

            var objSnippet = new StringBuilder().Append("return ").Append(entireClass.Item2);

            var ns = @"namespace currentNameSpace { ";

            //public static dynamic Cast(dynamic obj, Type castTo) { return Convert.ChangeType(obj, castTo); }

            var init = new StringBuilder().Append("public static class DynamicClass {  public static " + tgtClsName + " myFunc() {").Append(objSnippet).Append(" ;}}");

            var temp = new StringBuilder().Append(prefix).Append("\n").Append(ns).Append("\n").Append(init).Append("\n}");

            T generatedObj = default(T);

            try
            {
                var provider = CSharpCodeProvider.CreateProvider("c#");
                var options = new CompilerParameters();
                options.ReferencedAssemblies.Add("System.dll");
                options.ReferencedAssemblies.Add("System.Core.dll");

                var assemblyContainingNotDynamicClass = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                options.ReferencedAssemblies.Add(assemblyContainingNotDynamicClass);

                var results = provider.CompileAssemblyFromSource(options, new[]
                {
                    temp.ToString()
                });

                if (results.Errors.Count > 0)
                {
                    foreach (var error in results.Errors)
                    {
                        Console.WriteLine(error);
                    }
                }
                else
                {
                    var t = results.CompiledAssembly.GetType("currentNameSpace.DynamicClass");
                    generatedObj = (T)t.GetMethod("myFunc").Invoke(null, null);
                    return generatedObj;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }

            return default(T);
        }

        public static Tuple<string, string> ConstructEntireClassSnippet(string ip, SyntaxTree root, bool isChildFromArray, int rootStartPoint, List<PropsInfo> propInfoList)
        {
            var entireClass = "";
            string currentContext = "";

            currentContext = ip.Substring(root.Startpos + 1, (root.EndPos - root.Startpos) - 1);

            int sp = root.Startpos;
            var classNameBoundaryList = new List<int>();

            while (sp >= 0) // This while is to get the inner class name, mandatory
            {
                if (ip[sp] == '"')
                {
                    classNameBoundaryList.Add(sp);
                    if (classNameBoundaryList.Count() == 2)
                    {
                        break;
                    }
                }
                sp--;
            }

            if (root.ChildList.Count > 0)
            {
                var childClassList = new List<string>();
                var objConstructList = new List<string>();

                foreach (var child in root.ChildList)
                {
                    var snippet = ConstructEntireClassSnippet(ip, child, root.isArray, root.Startpos, propInfoList);
                    childClassList.Add(snippet.Item1);
                    objConstructList.Add(snippet.Item2);
                }
                entireClass += string.Join("\n", childClassList);

                //Contains json and the class structure. If it contains the json then go for the processing
                //Should have the class name to construct the class. Else this is the starting of the json. so exist
                //Logic to remove processed child class from json string and replace with corresponding c# class code

                for (var x = 0; x < childClassList.Count; x++)
                {
                    var start = 0;

                    if (root.isArray)
                    {
                        //Remove from start to end and replace with the details got from the previous step
                        currentContext = "";
                        break;
                    }

                    if (!root.ChildList[x].isArray)
                        start = currentContext.IndexOf('{');
                    else
                        start = currentContext.IndexOf('[');

                    var paramsMarkerList = FindRelaventParams(currentContext);

                    var end = paramsMarkerList[start];

                    var classNameDetails = new List<int>();

                    while (start >= 0) //Finding the class name inside the temp string
                    {
                        if (currentContext[start] == '"')
                        {
                            classNameDetails.Add(start);
                            if (classNameDetails.Count() == 2)
                            {
                                break;
                            }
                        }
                        start--;
                    }

                    var markingToDel = "";
                    if (classNameDetails.Any()) //If found then remove the temp class along with its prop
                    {
                        markingToDel = currentContext.Substring(classNameDetails[1], (end - classNameDetails[1]) + 1);
                    }

                    currentContext = currentContext.Replace(markingToDel, "");
                    currentContext = currentContext.Replace(",,", ",");
                }

                //Tinkering works here for removing accident characters here
                currentContext = currentContext.Replace(" ", "");

                if (currentContext.StartsWith(","))
                    currentContext = currentContext.TrimStart(',');

                if (currentContext.Any() && !currentContext.EndsWith(","))
                    currentContext += ",";

                var tempList = new List<string>();
                for (var i = 0; i < childClassList.Count; i++)
                {
                    var childClass = childClassList[i];
                    //  Extract class name from it
                    var classList = childClass.Split(' ');
                    var extractedclassname = classList[3]; // will  be class name  // since partial is introduced, //Ex: public partial class xxx

                    var currentStmt = "";
                    if (root.ChildList[i].isArray)
                        currentStmt = " public List<" + extractedclassname + "> " + extractedclassname + ";"; //{ get; set; } - have to pass this instead of ;
                    else
                        currentStmt = " public " + extractedclassname + " " + extractedclassname + ";";

                    tempList.Add(currentStmt);
                }
                currentContext += string.Join(",", tempList);

                var clsName = ip.Substring(classNameBoundaryList[1] + 1, (classNameBoundaryList[0] - classNameBoundaryList[1]) - 1);
                var currentCntxWrapper = "";

                if (isChildFromArray || root.isArray)
                {
                    currentCntxWrapper = "\"" + clsName + "\":[{" + currentContext + "}]";   //Construct class name with its props
                }
                else
                {
                    currentCntxWrapper = "\"" + clsName + "\":{" + currentContext + "}";   //Construct class name with its props
                }

                //Only customized input string will be passed here. 
                var classStruct = constructCurrentClass(currentCntxWrapper, currentCntxWrapper.IndexOf(currentContext) - 1, currentCntxWrapper.Length, root.isArray, isChildFromArray, objConstructList, 0, propInfoList);
                var unifiedLine = string.Join(" ", childClassList);
                entireClass = classStruct.Item1 + "\n" + unifiedLine; // Combine here the current class and already generated class structure.

                return new Tuple<string, string>(entireClass, classStruct.Item2);
            }
            else
            {
                //Complete input string will be passed here. 
                var classStruct = constructCurrentClass(ip, root.Startpos, root.EndPos, root.isArray, isChildFromArray, null, rootStartPoint, propInfoList);
                return classStruct;
            }
        }

        public static SyntaxTree constructParanthesisTree(string ip, int startpos, char symbolToSearch)
        {
            var currentTree = new SyntaxTree();
            currentTree.Startpos = startpos;
            currentTree.ChildList = new List<SyntaxTree>();
            startpos++;

            while (startpos < ip.Length)
            {
                if (ip[startpos] == symbolToSearch) //'}'
                {
                    currentTree.EndPos = startpos;
                    return currentTree;
                }
                else if (ip[startpos] == '[')
                {
                    var childLevel = constructParanthesisTree(ip, startpos, ']');
                    childLevel.isArray = true;

                    //var childList = childLevel.ChildList;
                    //var childList = childLevel.ChildList[0].ChildList; //As of now, we consider only the 1st element in array for processing
                    //childLevel.ChildList = childList;

                    currentTree.ChildList.Add(childLevel);
                    startpos = childLevel.EndPos + 1;
                }
                else if (ip[startpos] == '{') //symbolToSearch != ']'
                {
                    var childLevel = constructParanthesisTree(ip, startpos, '}');
                    currentTree.ChildList.Add(childLevel);
                    startpos = childLevel.EndPos + 1;
                }
                else
                {
                    startpos++;
                }
            }

            return currentTree;
        }

        //1st string - Class Structure
        //2nd string - Obj init structure
        public static Tuple<string, string> constructCurrentClass(string ip, int sp, int ep, bool isArray, bool isChildOfArray, List<string> objConstructedList, int rootStartPoint, List<PropsInfo> propInfoList)
        {
            string snippet = "";
            var objectConstruct = "";

            var currentSnpp = ip.Substring(sp, (ep - sp));

            currentSnpp = currentSnpp.Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "");
            var lineList = currentSnpp.Split(',');


            //Find "" from current startPos
            var classNameDetails = new List<int>();

            if (isChildOfArray)
            {
                if (rootStartPoint > 0)
                {
                    sp = rootStartPoint;
                }
                else //Replace this one by similar above one
                {
                    while (sp >= 0) //To find a opening array square brackets
                    {
                        if (ip[sp] == '[')
                        {
                            break;
                        }
                        sp--;
                    }
                }
            }

            while (sp >= 0) //To find a class name for curent props
            {
                if (ip[sp] == '"')
                {
                    classNameDetails.Add(sp);
                    if (classNameDetails.Count() == 2)
                    {
                        break;
                    }
                }
                sp--;
            }

            var className = ip.Substring(classNameDetails[1] + 1, (classNameDetails[0] - classNameDetails[1]));
            className = className.Replace("\"", "");

            foreach (var line in lineList)
            {
                var paramList = new List<string>();
                paramList.Add(line);

                var splitterIndx = line.IndexOf(':');
                if (splitterIndx > 0)
                {
                    //paramList = line.Split(':');
                    paramList[0] = line.Substring(0, splitterIndx); //prop name //instead of above line, replace this logic
                    paramList.Add(line.Substring(splitterIndx + 1)); // prop value 
                }

                if (paramList.Count > 1)
                {
                    var propName = paramList[0];
                    var value = paramList[1];

                    if (propName.Contains("\""))
                    {
                        propName = propName.Replace("\"", "");
                    }

                    //if (!string.IsNullOrEmpty(value) && value.Contains("\""))
                    //{
                    //    value = value.Replace("\"", "");
                    //}

                    if (propInfoList.Any(x => x.className == className && x.propName == propName))
                    {
                        //if (value.Contains("\""))
                        //{
                        //string //make for double etc..
                        var x = " public string " + propName + " { get; set; }";
                        snippet += x;

                        //objectConstruct += propName + "= \"" + value + "\"," + "\n"; //As of now for string alone
                        objectConstruct += propName + " = " + value + "," + "\n"; //As of now for string alone
                        //}
                    }
                }
                else
                {
                    // public List<InnerArry> InnerArry_obj ;  // public InnerArry InnerArry_obj ; 

                    var currentStr = paramList[0];
                    if (!string.IsNullOrWhiteSpace(currentStr))
                    {
                        snippet += currentStr.Replace(";", " { get; set; }");
                        var statList = currentStr.Replace(',', ' ').Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var currentObjInit = "";

                        statList[2] = statList[2].Replace(";", "");
                        if (isArray)
                        {
                            currentObjInit = objConstructedList[0] + ",";
                        }
                        else
                        {
                            if (propInfoList.Any(x => x.className == className && x.propName == statList[2]))
                            {
                                currentObjInit = statList[2] + " = " + objConstructedList[0] + ",";
                            }
                        }

                        objectConstruct += "\n" + currentObjInit;
                        objConstructedList.RemoveAt(0);
                    }
                }
            }

            var currentSnippet = "public partial class " + className + " { " + snippet + " } ";

            var currentObjCons = "";
            if (isArray)
            {
                currentObjCons = "new List<" + className + ">()\n { \n" + objectConstruct.TrimEnd(new char[] { ',' }) + "\n}\n";
            }
            else
            {
                currentObjCons = "new " + className + "()\n { \n" + objectConstruct.TrimEnd(new char[] { ',' }) + "\n}\n";
            }

            return new Tuple<string, string>(currentSnippet, currentObjCons);
        }

        public static Dictionary<int, int> FindRelaventParams(string ip)
        {
            var startEndParamsPosList = new Dictionary<int, int>();
            var startingparanthesisList = new List<int>();
            var x = 0;

            while (x < ip.Length)
            {
                if (ip[x] == '{' || ip[x] == '[')
                {
                    startingparanthesisList.Add(x);
                }
                else if (ip[x] == '}' || ip[x] == ']')
                {
                    startEndParamsPosList.Add(startingparanthesisList.Last(), x);
                    startingparanthesisList.Remove(startingparanthesisList.Last());
                }
                x++;
            }

            return startEndParamsPosList;
        }

    }
}
