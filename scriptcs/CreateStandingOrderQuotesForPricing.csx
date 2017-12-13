#load "InputUtilities.csx"

using Aptify.Framework.Application;
using Aptify.Framework.DataServices;
using Aptify.Framework.ExceptionManagement;
using Aptify.Framework.BusinessLogic;
using Aptify.Framework.BusinessLogic.GenericEntity;
using System.Data;

const string quoteEffectiveDate = "1/2/2018";
const string quotePoNumber = "RENEWALPRICECHANGE";
var aptifyPass = Environment.GetEnvironmentVariable("AptifyPassword");
var aptifyServerName = Environment.GetEnvironmentVariable("AptifyServer");
var uc = new UserCredentials(aptifyServerName, "Aptify", "Aptify", false, -1, "sa", aptifyPass, null, false, -1, true);
var da = new DataAction(uc);
var app = new AptifyApplication(uc);

var standingOrderTable = da.GetDataTable(string.Format("select top 1 so.Id, so.ShipToId " +
    "from vwStandingOrders so " +
    "where so.OSBAStandingOrderCombinationStatus = 'Complete'" +
    "and so.Status = 'Active'" +
    "and so.Frequency = 'Monthly'" + 
    "and so.ShipToId not in (select ShipToId from vwOrders o where o.PONumber = '{0}')", quotePoNumber));

foreach (var standingOrderRow in standingOrderTable.Rows) {
    var standingOrderId = System.Convert.ToInt32(standingOrderRow["ID"]);
    var personId = System.Convert.ToInt32(standingOrderRow["ShipToId"]);

    Console.WriteLine(string.Format("Found Standing Order:{0} for PersonId:{1}", standingOrderId, personId));

    var standingOrderProductTable = da.GetDataTable(string.Format("select * 
        " From vwStandingOrProd p " +
        " where p.StandingOrderId = {0}", standingOrderId));

    if (standingOrderProductTable.Rows.Count == 0)
        continue;

    Console.WriteLine("Creating New Order...");
    var quoteGe = app.GetEntityObject("Orders", -1);

    quoteGe.SetValue("ShipToId", personId);
    quoteGe.SetValue("OrderDate", quoteEffectiveDate);
    quoteGe.SetValue("PONumber", quotePoNumber);
    quoteGe.SetValue("OrderTypeID", "4"); // ' Default to Quotation
    quoteGe.SetValue("OrderSourceID", "1"); // ' Default to Regular
    quoteGe.SetValue("OrderStatusID", "1"); // ' Default to Taken
    quoteGe.SetValue("PayTypeID", 1); // ' Default to Regular

    foreach (var standingOrderProductRow in standingOrderProductTable.Rows) {
        var productId = System.Convert.ToInt32(standingOrderProductRow["ProductId"]);
        var quoteLineGe = (AptifyGenericEntityBase)quoteGe.SubTypes["OrderLines"].Add();
        quoteLineGe.SetValue("ProductId", productId);
        quoteLineGe.SetValue("Quantity", 1);

        Console.WriteLine(string.Format("Adding Product {0}", productId));
    }

    var errorString = string.Empty;
    if (!quoteGe.Save(false, ref errorString))
        Console.WriteLine(string.Format("Created OrderId:{0}", quoteGe.RecordID));
    else
        Console.WriteLine(string.Format("Error Saving Standing Order: {0}", errorString));
}

