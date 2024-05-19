using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

//public class getfromaid : MonoBehaviour
//{
namespace getfromid
{
    public class getfromid : MonoBehaviour
    {
        public JObject get_aid_from_json(string jsonfile, int aid)
        {
            StreamReader sr = new StreamReader(jsonfile);
            string datastr = sr.ReadToEnd();
            sr.Close();

            JObject jsonObj = JObject.Parse(datastr);
            return jsonObj;
        }
    }
}
//}