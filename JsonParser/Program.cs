using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace JsonParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputText = File.ReadAllText("TextFile1.txt");

            var generatedObj = ConvertJsonStrToClassStructure(inputText);

            Console.WriteLine(generatedObj.ClassSnippet);  // Generated Class structure
            Console.WriteLine(generatedObj.ObjectSnippet); // Generated object initialization structure
            Console.WriteLine(generatedObj.GeneratedObject); //Generated runtime object 

            Console.ReadLine();
        }

        public static dynamic ConvertJsonStrToClassStructure(string jsonInput)
        {
            var txt = "\"Root\":" + jsonInput.Trim().Replace(" ", "").Replace("\n", "").Replace("\t", "").Replace("\r", "");

            var paranthesisTree = constructParanthesisTree(txt, txt.IndexOf('{'), '}');
            var entireClass = ConstructEntireClassSnippet(txt, paranthesisTree);

            var prefix = @"using System;
            using System.Collections.Generic;
            using System.Text;
            using System.IO;
            using Microsoft.CSharp;
            using System.CodeDom.Compiler;
            using System.Reflection;
            using System.Linq; ";

            var objSnippet = "return " + entireClass.Item2;
            var classSnippet = entireClass.Item1;

            var ns = @"namespace currentNameSpace
            { ";

            var init = "public class DynamicClass { public static Root myFunc() {" + objSnippet + ";}}";

            var temp = prefix + "\n" + ns + "\n" + classSnippet + "\n" + init + "\n}";

            object generatedObj = null;

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
                    temp
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
                    //var obj = Activator.CreateInstance(t);
                    generatedObj = t.GetMethod("myFunc").Invoke(null, null);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message);
            }

            return new { ClassSnippet = entireClass.Item1, ObjectSnippet = entireClass.Item2, GeneratedObject = generatedObj };
        }

        public static Tuple<string, string> ConstructEntireClassSnippet(string ip, SyntaxTree root)
        {
            var entireClass = "";

            string temp = "";
            if (root.isArray)
            {
                temp = ip.Substring(root.Startpos, (root.EndPos - root.Startpos) + 1);

                //Lets remove all the subclass in the array except the first one
                var paramList = FindRelaventParams(temp);
                var endIndex = paramList[1];

                if (temp[endIndex + 1] == ']')
                {
                    //end of array
                }
                else
                {
                    //Non end of array. lets remove the remaining part.
                    temp = temp.Substring(0, endIndex + 1);
                    temp += ']';
                }
            }
            else
            {
                temp = ip.Substring(root.Startpos + 1, (root.EndPos - root.Startpos) - 1);
            }
            //string temp = ip.Substring(root.Startpos, (root.EndPos - root.Startpos));

            if (root.isArray) //Remove the { and } from the temp
            {
                temp = temp.TrimStart('[').TrimEnd(']');
                temp = temp.TrimStart('{').TrimEnd('}');
            }

            int sp = root.Startpos;
            var classNameDetailList = new List<int>();

            while (sp >= 0) // This while is to check the inner class name, if exists!
            {
                if (ip[sp] == '"')
                {
                    classNameDetailList.Add(sp);
                    if (classNameDetailList.Count() == 2)
                    {
                        break;
                    }
                }
                sp--;
            }


            if (root.ChildList.Count > 0) //if while is true then this if will be true
            {
                var childClassList = new List<string>();
                var objConstructList = new List<string>();

                foreach (var child in root.ChildList)
                {
                    var snippet = ConstructEntireClassSnippet(ip, child);
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

                    if (!root.ChildList[x].isArray)
                        start = temp.IndexOf('{');
                    else
                        start = temp.IndexOf('[');

                    var paramsMarkerList = FindRelaventParams(temp);

                    var end = paramsMarkerList[start];

                    var classNameDetails = new List<int>();

                    while (start >= 0) //Finding the class name inside the temp string
                    {
                        if (temp[start] == '"')
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
                        markingToDel = temp.Substring(classNameDetails[1], (end - classNameDetails[1]) + 1);
                    }

                    temp = temp.Replace(markingToDel, "");
                    temp = temp.Replace(",,", ",");
                }

                temp = temp.Replace(" ", "");

                if (temp.StartsWith(","))
                    temp = temp.TrimStart(',');

                if (temp.Any() && !temp.EndsWith(","))
                    temp += ",";

                var tempList = new List<string>();
                for (var i = 0; i < childClassList.Count; i++)
                {
                    var childClass = childClassList[i];
                    //  Extract class name from it
                    var classList = childClass.Split(' ');
                    var extractedclassname = classList[2]; //will  be class name

                    var currentStmt = "";
                    if (root.ChildList[i].isArray)
                        // currentStmt = " public List<" + extractedclassname + "> " + extractedclassname + "_obj ;";
                        currentStmt = " public List<" + extractedclassname + "> " + extractedclassname + ";";
                    else
                        currentStmt = " public " + extractedclassname + " " + extractedclassname + ";";

                    tempList.Add(currentStmt);
                }
                temp += string.Join(",", tempList);

                var clsName = ip.Substring(classNameDetailList[1] + 1, (classNameDetailList[0] - classNameDetailList[1]) - 1);
                var xx = "\"" + clsName + "\":{" + temp + "}";   //Construct class name with its props
                var classStruct = constructCurrentClass(xx, xx.IndexOf(temp) - 1, xx.Length, root.isArray, objConstructList);
                var unifiedLine = string.Join(" ", childClassList);
                entireClass = classStruct.Item1 + "\n" + unifiedLine; // Combine here the current class and already generated class structure.

                return new Tuple<string, string>(entireClass, classStruct.Item2);
            }
            else
            {
                var classStruct = constructCurrentClass(ip, root.Startpos, root.EndPos, root.isArray, null);
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

                    var childList = childLevel.ChildList[0].ChildList;
                    childLevel.ChildList = childList;

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
        public static Tuple<string, string> constructCurrentClass(string ip, int sp, int ep, bool isArray, List<string> objConstructedList)
        {
            string snippet = "";
            var objectConstruct = "";

            var currentSnpp = ip.Substring(sp, (ep - sp));
            if (isArray)
            {
                var arryList = currentSnpp.Split(new string[] { "}," }, StringSplitOptions.None);
                arryList[0] += "}";
                arryList[0] = arryList[0].TrimStart('[');
                currentSnpp = arryList[0];
            }

            currentSnpp = currentSnpp.Replace("{", "").Replace("}", "").Replace("[", "").Replace("]", "");
            var lineList = currentSnpp.Split(',');

            foreach (var line in lineList)
            {
                var paramList = line.Split(':');
                if (paramList.Length > 1)
                {
                    if (paramList[1].Contains("\""))
                    {
                        //string //make for double etc..
                        var x = " public string " + paramList[0].Replace("\"", "") + " { get; set; }";
                        snippet += x;

                        objectConstruct += paramList[0].Replace("\"", "") + "= \"" + paramList[1].Replace("\"", "") + "\"," + "\n"; //As of now for string alone
                    }
                }
                else
                {
                    // public List<InnerArry> InnerArry_obj ;
                    var currentStr = paramList[0];
                    if (!string.IsNullOrWhiteSpace(currentStr))
                    {
                        snippet += currentStr.Replace(";", " { get; set; }");
                        var statList = currentStr.Replace(',', ' ').Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var currentObjInit = "";

                        statList[2] = statList[2].Replace(";", "");
                        if (currentStr.Contains("List"))
                        {                            
                            //currentObjInit = statList[2] + " = new List<" + statList[2].Replace("_obj", "") + ">()\n{\n" + objConstructedList[0] + "\n},";
                            currentObjInit = statList[2] + " = new List<" + statList[2] + ">(1)\n{\n" + objConstructedList[0] + "\n},";
                        }
                        else
                        {
                            currentObjInit = statList[2] + " = " + objConstructedList[0] + ",";
                        }

                        objectConstruct += "\n" + currentObjInit;
                        objConstructedList.RemoveAt(0);
                    }
                }
            }

            //Find "" from current startPos
            var classNameDetails = new List<int>();

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

            var currentSnippet = "public class " + className + " { " + snippet + " } ";
            //currentSnippet = currentSnippet.Replace("\"", "");

            var currentObjCons = "new " + className + "()\n { \n" + objectConstruct.TrimEnd(new char[] { ',' }) + "\n}\n";

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
