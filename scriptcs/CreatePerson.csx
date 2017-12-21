using Aptify.Framework.Application;
using Aptify.Framework.DataServices;
using Aptify.Framework.ExceptionManagement;
using Aptify.Framework.BusinessLogic;
using Aptify.Framework.BusinessLogic.GenericEntity;
using System.Data;
using System.Collections;
using System.Collections.Generic;

var aptifyPass = Environment.GetEnvironmentVariable("StgAptifyDbPassword");
var uc = new UserCredentials("stgaptifydb.ohiobar.org", "Aptify", "Aptify", false, -1, "sa", aptifyPass, null, false, -1, true);
var da = new DataAction(uc);

Console.WriteLine("Booting Aptify");
var app = new AptifyApplication(uc);
Console.WriteLine("Aptify Loaded");
 
// create a new person 
var person = app.GetEntityObject("Persons", -1);
person.SetValue("Email", "someuser@somedomain.com");
person.SetValue("FirstName", "John");
person.SetValue("LastName", "Smith");
person.SetValue("SCNUM", "0004567"); //optional
person.SetValue("Birthday", "12/21/1985");

var errorString = string.Empty;
if (!person.Save(false, ref errorString))
    throw new Exception(errorString);

Console.WriteLine($"Created Person {person.RecordID}");
 
// Aptify API creates web user object and passes it back to the caller
var webuser = app.GetEntityObject("Web Users", -1);
webuser.SetValue("LastName", "Smith");
webuser.SetValue("Email", "someuser@somedomain.com");
webuser.SetValue("LinkID", person.RecordID); //person.GetValue("ID")
webuser.SetValue("UserID", "jsmith");
webuser.SetValue("PWD", "password1");
 
if (!webuser.Save(false, ref errorString))
    throw new Exception(errorString);

Console.WriteLine($"Created Web User {webuser.RecordID}");