using System.Collections.Generic;

namespace xx1
{
    #region TextFile1.txt

    public partial class Text1Root { public service service { get; set; } }
    public partial class service
    {
        public string id { get; set; } public string port { get; set; } public string Inbetween { get; set; }
        public checks checks { get; set; } public List<checks_4> checks_4 { get; set; }
    }

    public partial class checks
    {
        public string name { get; set; } public string NewOne { get; set; }
        public checks_1 checks_1 { get; set; }
    }

    public partial class checks_1
    {
        public string interval { get; set; } public string timeout { get; set; }
        public checks_2 checks_2 { get; set; } public List<checks_3> checks_3 { get; set; }
    }
    public partial class checks_2 { public string name { get; set; } }
    public partial class checks_3 { public string name1 { get; set; } public string NewAdd { get; set; } }
    public partial class checks_4 { public string interval { get; set; } public string timeout { get; set; } }

    #endregion
}

namespace xx
{
    #region TextFile9.txt
    public class Root
    {
        public string xxxx { get; set; }
        //public string xxxx1 { get; set; }
        public medications obj { get; set; }
        //public List<medications> medications { get; set; }
        public List<labs> labs { get; set; } public List<imaging> imaging { get; set; }
    }

    public class medications
    {
        public List<aceInhibitors> aceInhibitors { get; set; }
        public List<anticoagulants> anticoagulants { get; set; }
    }

    public class aceInhibitors
    {
        public string name { get; set; } public string strength { get; set; } public string dose { get; set; }
        public string route { get; set; } public string sig { get; set; } public string pillCount { get; set; } public string refills { get; set; }
    }

    public class anticoagulants
    {
        public string name { get; set; } public string strength { get; set; } public string dose { get; set; }
        public string route { get; set; } public string sig { get; set; } public string pillCount { get; set; } public string refills { get; set; }
    }

    public class labs { public string name { get; set; } public string time { get; set; } public string location { get; set; } public string name1 { get; set; } public string time1 { get; set; } public string location1 { get; set; } }

    public class imaging { public string name { get; set; } public string time { get; set; } public string location { get; set; } }

    #endregion

    #region TextFile2.txt
    public partial class Text2Root { public checks_1 checks_1 { get; set; } }
    public partial class checks_1
    {
        public string interval { get; set; } public string timeout { get; set; }
        public checks_2 checks_2 { get; set; } public checks_3 checks_3 { get; set; }
    }
    public partial class checks_2 { public string name { get; set; } }
    public partial class checks_3 { public string name1 { get; set; } }
    #endregion

    #region TextFile3.txt
    public partial class Text3Root { public List<service> service { get; set; } public string AnotheerEx { get; set; } public List<checks5> checks5 { get; set; } }

    public partial class service
    {
        public string idxxx { get; set; }
        public string id { get; set; } public string address { get; set; } public string port { get; set; }
        public List<checks> checks { get; set; } public List<checks1> checks1 { get; set; }
    }

    public partial class checks
    {
        public string name { get; set; } public string http { get; set; } public string interval { get; set; } public string timeout { get; set; }
        public List<newCheckOne> newCheckOne { get; set; }
    }

    public partial class newCheckOne
    {
        public string interval_Last_1_1 { get; set; }
        public string timeout_Last_1_1 { get; set; }
        public string interval_Last { get; set; }
        public string timeout_Last { get; set; }
        public List<InnerArry> InnerArry { get; set; }
    }

    public partial class InnerArry { public string interval_Last_1 { get; set; } public string timeout_Last_1 { get; set; } public InnerArry InnerArry1 { get; set; } }

    public partial class checks1 { public string name { get; set; } public string http { get; set; } public checks1 checks11 { get; set; } public checks1 check1s1 { get; set; } }

    public partial class checks5
    {
        public string name_1 { get; set; }
        public string http_1 { get; set; }
        public string name_2 { get; set; }
        public string http_2 { get; set; }
        public string name { get; set; } public string http { get; set; } public checks5 checks15 { get; set; } public checks5 check1s5 { get; set; }
    }
    #endregion

    #region TextFile4.txt

    public partial class Text4Root { public employees employees { get; set; } }

    public partial class employees
    {
        public string Test { get; set; }
        public List<employee> employee { get; set; }
    }

    public partial class employee
    {
        public int numberhere { get; set; }
        public string id { get; set; } public string firstName { get; set; }
        public string lastName { get; set; } public string photo { get; set; }
        public employee employee_1 { get; set; }
        public employee employee_2 { get; set; }
    }

    #endregion

    #region TextFile5.txt

    public partial class Text5Root
    {
        public int id { get; set; } public string type { get; set; }
        public string name { get; set; }
        public double ppu { get; set; }
        public batters batters { get; set; }
        public List<topping> topping { get; set; }
    }

    public partial class batters
    {
        public List<batter> batter { get; set; }
    }

    public partial class batter
    {
        public string id { get; set; } public string type { get; set; }
        public string id_1 { get; set; } public string type_1 { get; set; }
        public string id_2 { get; set; } public string type_2 { get; set; }
        public string id_3 { get; set; } public string type_3 { get; set; }
    }

    public partial class topping
    {
        public string id { get; set; } public string type { get; set; }
        public string id_1 { get; set; }
        public string id_2 { get; set; }
        public string id_3 { get; set; }
        public string id_4 { get; set; } public string id_5 { get; set; }
        public string id_6 { get; set; }
    }
    #endregion

    #region TextFile7.txt

    public partial class Text7Root { public List<Students> Students { get; set; } }

    public partial class Students
    {
        public string Name { get; set; } public string Major { get; set; } public string SomeOther { get; set; }
        public string xxxx { get; set; }
    }

    #endregion

    #region TextFile8.txt
    public partial class Text8Root { public sammy sammy { get; set; } public jesse jesse { get; set; } public drew drew { get; set; } public jamie jamie { get; set; } }
    public partial class sammy { public string username { get; set; } public string location { get; set; } public bool online { get; set; } public int followers { get; set; } }
    public partial class jesse { public string username { get; set; } public string location { get; set; } public bool online { get; set; } public int followers { get; set; } }
    public partial class drew { public string username { get; set; } public string location { get; set; } public bool online { get; set; } public int followers { get; set; } }
    public partial class jamie { public string username { get; set; } public string location { get; set; } public bool online { get; set; } public int followers { get; set; } }
    #endregion

}



