
<b>What is the use of this app?</b><br/>
      Sometimes you will need to find the equivalent class structure of a Json string/data to deserialize or for some purpose. You can find the Json 2 Csharp gen online too. But what if, you need a pluggable solution?. There this can be used to get your solution. <br/>

<b>How to convert the Json To Class?</b><br/>
     We are having a method named "ConvertJsonStrToClassStructure" inside the Program.cs. Call this function with a valid json data and you will be getting the desired Class structure as a result.
  
<b>Any limitations/restriction with this app?</b><br/>
      Yes. A small limitations are there. <br/>
      1. As of now, this app will process 1st element in Json array input. Again this first element can be another array of simple key value pair. This limitation is applicable at all levels recursively. No limitations for the normal class structure inside the Json text. Well not a problem. Take a look at the below examples on Case 3 and Case 4, you will understand more on it.  
      2. Will process only Key Value Pair alone in the Json text. (Ie, "key" : "Value" is a valid in this context and [{"xxx","xx1"}] is an invalid context as of now). <br/>
      3. Only string values are considered, Will make support for different types like int, float, etc. (Ie. "Key": "Value", U can able to see a output like public string Key and for the input "Key":333.33 you will not get any generated structure)
      4. This app assumes you are passing a valid Json input. Donot crash the app as of now :)  
      5. Will address the above ones in upcoming checkins for sure.<br/>
      
<b>Any working samples available?</b> <br/>
   Yup. Here you go. Lets start with a simple one. <br/><br/>

<b>Case 1: </b>
<pre>
   {
"checks_1": {
				"interval": "5s",
				"timeout": "150s",
				"checks_2": {
					"name": "http_check_name"
				},
				"checks_3": {
					"name1": "http_check_name"
				}
			}
}
</pre>

<b>Generated Structure:</b>
<pre>
public class Root { public checks_1 checks_1_obj; }
public class checks_1 { public string interval; public string timeout; public checks_2 checks_2_obj; public checks_3 checks_3_obj; }
public class checks_2 { public string name;  }
public class checks_3 { public string name1;  }
</pre>


<b>Case 2:</b>

<pre>
{
	"employees": {
	   "Test" : "Hi",
		"employee": [
			{
				"id": "1",
				"firstName": "Tom",
				"lastName": "Cruise",
				"photo": "https://pbs.twimg.com/profile_images/735509975649378305/B81JwLT7.jpg"
			},
			{
				"id": "2",
				"firstName": "Maria",
				"lastName": "Sharapova",
				"photo": "https://pbs.twimg.com/profile_images/3424509849/bfa1b9121afc39d1dcdb53cfc423bf12.jpeg"
			},
			{
				"id": "21",
				"firstName": "Maria1",
				"lastName": "Sharapova1",
				"photo": "https://pbs.twimg.com/profile_images/3424509849/bfa1b9121afc39d1dcdb53cfc423bf12.jpeg"
			}			
		]
	}
}
</pre>

<b>Generated Structure:</b>
<pre>
public class Root { public employees employees_obj; }
public class employees { public string Test; public List&ltemployee&gt employee_obj; }
public class employee { public string id; public string firstName; public string lastName; public string photo;  }
</pre>


<b>Case 3:</b>
<pre>
{
	"id": "0001",
	"type": "donut",
	"name": "Cake",
	"ppu": "0.55",
	"batters":
		{
			"batter":
				[
					{ "id": "1001", "type": "Regular" },
					{ "id_1": "1002", "type_1": "Chocolate" },
					{ "id_2": "1003", "type_2": "Blueberry" },
					{ "id_3": "1004", "type_3": "Devil's Food" }
				]
		},
	"topping":
		[
			{ "id": "5001", "type": "None" },
			{ "id_1": "5002", "type": "Glazed" },
			{ "id_2": "5005", "type": "Sugar" },
			{ "id_3": "5007", "type": "Powdered Sugar" },
			{ "id_4": "5006", "type": "Chocolate with Sprinkles" },
			{ "id_5": "5003", "type": "Chocolate" },
			{ "id_6": "5004", "type": "Maple" }
		]
}
</pre>


<b>Generated Structure:</b>
<pre>
public class Root { public string id; public string type; public string name; public string ppu; public batters batters_obj; public List&lttopping&gt topping_obj; }
public class batters { public List&ltbatter&gt batter_obj; }
public class batter { public string id; public string type;  }
public class topping { public string id; public string type;  }
</pre>


<b>Case 4:-</b>
<pre>
{
	"service": [{
		"id": "293781839_3434_New_soemthig",		
		"address": "127.0.0.1",
		"port": "64921",
		"checks": [{
			"name": "http_check_name",
			"http": "http://127.0.0.1:62133/Service1.svc/MyService/PingMe",
			"interval": "5s",
			"timeout": "150s",
			"newCheckOne" : [{
				"interval_Last": "5s",
				"timeout_Last": "150s",
				    "InnerArry": [{
				        "interval_Last_1": "5s",
				         "timeout_Last_1": "150s"
				           }]
			},
			{
				"interval_Last_1": "5s",
				"timeout_Last_1": "150s"
			}]
		}],
		"address": "127.0.0.1",
		"checks1": [{
			"name": "11",
			"http": "11",
		},
		{
			"name_1": "12",
			"http_1": "12",
		}],
		"idxxx": "293781839_3434_New_soemthig"	
	}],
	"AnotheerEx" : "xxxx",
	"checks5": [{
			"name_2": "11",
			"http_2": "11",
		},
		{
			"name_1": "12",
			"http_1": "12",
		}]
}
</pre>


<b>Generated Structure:</b>
<pre>
public class Root { public string AnotheerEx; public List&ltservice&gt service_obj; public List&ltchecks5&gt checks5_obj; }
public class service { public string id; public string address; public string port; public string address; public string idxxx; public List&ltchecks&gt checks_obj; public List&ltchecks1&gt checks1_obj; }
public class checks { public string name; public string http; public string interval; public string timeout; public List&ltnewCheckOne&gt newCheckOne_obj; }
public class newCheckOne { public string interval_Last; public string timeout_Last; public List&ltInnerArry&gt InnerArry_obj; }
public class InnerArry { public string interval_Last_1; public string timeout_Last_1;  }
public class checks1 { public string name; public string http;  }
public class checks5 { public string name_2; public string http_2;  }
</pre>

</br>

<b> Any thing else?</b></br>
    Comment/ping, if found any bug in this solution. Much happy to find and fix it out. Thanks in advance for using this tool. Have a great day ahead. Bye. </br>
