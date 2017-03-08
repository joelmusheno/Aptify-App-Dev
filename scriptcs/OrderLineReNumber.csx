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

var orderQueryString = "select top 1  od.OrderID " + 
    " from vwOrderDetails od " + 
    " join vwProducts p on od.ProductId = p.Id " +
    " where p.DefaultDuesProduct =1  and od.Sequence > 1" + 
    " and od.OrderId = 1477251";
var ordersTable = da.GetDataTable(orderQueryString);

foreach(DataRow orderRow in ordersTable.Rows) {
    
    var orderId = Convert.ToInt64(orderRow["OrderID"]);

    Console.WriteLine(string.Format("OrderId:{0}", orderId));

    var orderGe = app.GetEntityObject("Orders", orderId);
    var orderLinesSubType = orderGe.SubTypes["OrderLines"];
    
    for(var i = 0; i < orderLinesSubType.Count; i++) {
        var orderLineGe = (AptifyGenericEntityBase)orderLinesSubType[i];

        var productId = Convert.ToInt64(orderLineGe.GetValue("ProductID"));
        var productGe = app.GetEntityObject("Products", productId);

        Console.WriteLine(string.Format("RecordId:{0} | ProductId:{1} | DefaultDuesProduct:{2}", 
            orderLineGe.RecordID, orderLineGe.GetValue("ProductID"), 
            Convert.ToBoolean(productGe.GetValue("DefaultDuesProduct"))));

        if (Convert.ToBoolean(productGe.GetValue("DefaultDuesProduct"))) 
            orderLineGe.SetValue("Sequence", 1);
    }

    var errorString = string.Empty;
    if (!orderGe.Save(false, ref errorString))
        throw new Exception(errorString);
}

Console.WriteLine("Complete")
