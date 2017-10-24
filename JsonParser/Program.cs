using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JsonParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputText = File.ReadAllText("TextFile9.txt");

            var classStructure = ConvertJsonStrToClassStructure(inputText);

            Console.WriteLine(classStructure);
            Console.ReadLine();
        }

        public static string ConvertJsonStrToClassStructure(string jsonInput)
        {
            var txt = "\"Root\":" + jsonInput.Trim().Replace(" ", "").Replace("\n", "").Replace("\t", "").Replace("\r", "");

            var paranthesisTree = constructParanthesisTree(txt, txt.IndexOf('{'), '}');
            var entireClass = ConstructEntireClassSnippet(txt, paranthesisTree);

            return entireClass;
        }

        public static string ConstructEntireClassSnippet(string ip, SyntaxTree root)
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
                foreach (var child in root.ChildList)
                {
                    var snippet = ConstructEntireClassSnippet(ip, child);
                    childClassList.Add(snippet);
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
                        currentStmt = " public List<" + extractedclassname + "> " + extractedclassname + "_obj ;";
                    else
                        currentStmt = " public " + extractedclassname + " " + extractedclassname + "_obj ;";

                    tempList.Add(currentStmt);
                }

                temp += string.Join(",", tempList);

                var clsName = ip.Substring(classNameDetailList[1] + 1, (classNameDetailList[0] - classNameDetailList[1]) - 1);    
                var xx = "\"" + clsName + "\":{" + temp + "}";   //Construct class name with its props
                var classStruct = constructCurrentClass(xx, xx.IndexOf(temp) - 1, xx.Length, root.isArray);
                var unifiedLine = string.Join(" ", childClassList);
                entireClass = classStruct + unifiedLine; // Combine here the current class and already generated class structure.

                return entireClass;
            }
            else
            {
                var classStruct = constructCurrentClass(ip, root.Startpos, root.EndPos, root.isArray);
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

        public static string constructCurrentClass(string ip, int sp, int ep, bool isArray)
        {
            string snippet = "";

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
                        var x = "public string " + paramList[0].Replace("\"", "") + " ; ";
                        snippet += x;
                    }
                }
                else
                {
                    snippet += paramList[0];
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

            var currentSnippet = "public class " + ip.Substring(classNameDetails[1] + 1, (classNameDetails[0] - classNameDetails[1])) + " { " + snippet + " } ";
            currentSnippet = currentSnippet.Replace("\"", "");
            return currentSnippet;
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
