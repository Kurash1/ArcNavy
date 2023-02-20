using System.Text.RegularExpressions;

namespace ArcShared
{
    class Arc
    {
        public static void LoadClasses(string file, string classtype, Dictionary<string, Dictionary<string, Dictionary<string,string>>> classes)
        {
            List<string> classcode = ParseString(Regex.Replace(file, "#.*", ""));
            int indent = 0;
            string keyholder = "";
            string id = "";

            if (!classes.ContainsKey(classtype))
                classes.Add(classtype, new Dictionary<string, Dictionary<string, string>>());

            Dictionary<string, string> elements = new Dictionary<string, string>();

            for (int i = 0; i < classcode.Count; i++)
            {
                string g = classcode[i];
                switch (g)
                {
                    case "}":
                        indent--;
                        if (indent == 0)
                        {
                            elements.Add("id", id);
                            classes[classtype].Add(id, new Dictionary<string, string>(elements));
                        }
                        else if (indent >= 2)
                            elements[keyholder] += "} ";
                        break;
                    case "{":
                        indent++;
                        if (indent >= 2)
                            elements[keyholder] += "{ ";
                        break;
                    case string s when indent == 0:
                        elements.Clear();
                        id = s;
                        i++; while (!_expect(i, "=")) { i++; }
                        i++; while (!_expect(i, "{")) { i++; }
                        indent++;
                        break;
                    case string s when indent == 1:
                        keyholder = s;
                        i++; while (!_expect(i, "=")) { i++; }
                        i++; if (classcode[i] == "{")
                        {
                            keyholder = s;
                            indent++;
                            elements.Add(keyholder, "");
                        }
                        else
                            elements.Add(s, classcode[i]);
                        break;
                    case string s when indent >= 2:
                        elements[keyholder] += s + " ";
                        break;
                }
            }
            bool _expect(int index, string regex, string error = "")
            {
                return actual_except(classcode[index], index, regex, error);
            }

            return;
        }
        static bool actual_except(string str, int index, string regex, string error)
        {
            if (Regex.IsMatch(str, regex))
                return true;
            if (str == "\\t" || str == "\\n")
                return false;

            if (error == "")
                Console.WriteLine(new Exception("Expecting " + regex + " at index " + index + " was " + str));
            else
                Console.WriteLine(error);

            throw new Exception("Arc Exception");
        }
        public static List<string> ParseString(string str)
        {
            var retval = new List<string>();
            if (String.IsNullOrWhiteSpace(str)) return retval;
            int ndx = 0;
            string s = "";
            bool insideDoubleQuote = false;
            int indent = 0;

            while (ndx < str.Length)
            {
                if ((str[ndx] == ' ' || str[ndx] == '\n' || str[ndx] == '\t') && !insideDoubleQuote && indent == 0)
                {
                    if (!String.IsNullOrWhiteSpace(s.Trim())) retval.Add(s.Trim());
                    s = "";
                }
                if (str[ndx] == '"') insideDoubleQuote = !insideDoubleQuote;
                if (str[ndx] == '(') indent++;
                if (str[ndx] == ')') indent--;
                s += str[ndx];
                ndx++;
            }
            if (!String.IsNullOrWhiteSpace(s.Trim())) retval.Add(s.Trim());
            return retval;
        }
    }
}
