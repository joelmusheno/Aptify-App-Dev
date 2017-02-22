using Aptify.Framework.Application;
using Aptify.Framework.DataServices;
using Aptify.Framework.ExceptionManagement;
using Aptify.Framework.BusinessLogic;
using Aptify.Framework.BusinessLogic.GenericEntity;
using System.Data;

public static class InputUtilities {
    public static int GetUserInputInteger(string prompt) {
        int result = -1;
        Console.WriteLine(prompt);
        var input = Console.ReadLine();
        if (!int.TryParse(input, out result))
            throw new ArgumentException(input);
        return result;
    }
}

string lastBillingDate = "3/20/17";

var aptifyPass = Environment.GetEnvironmentVariable("AptifyPassword");
var uc = new UserCredentials("aptifydb.ohiobar.org", "Aptify", "Aptify", false, -1, "sa", aptifyPass, null, false, -1, true);
var da = new DataAction(uc);
var app = new AptifyApplication(uc);
var standingOrderId = InputUtilities.GetUserInputInteger("Please Enter Standing Order Id.");
var queryStandingOrderLines = string.Format("select * from vwStandingOrderSchedule where StandingOrderID = {0}",  standingOrderId);
var standingOrderLinesTable = da.GetDataTable(queryStandingOrderLines);

if (standingOrderLinesTable.Rows.Count == 1) {
    var row = standingOrderLinesTable.Rows[0];

    Console.WriteLine("--- Standing Order Schedule Information ---");
    Console.WriteLine("Single Row in Standing Order Table");

    Console.WriteLine(string.Format("ScheduledDate: {0} | IsComplete: {1} ", row["ScheduledDate"], bool.Parse(Convert.ToString(row["IsComplete"]))));

    Console.WriteLine("--- Order Information ---");

    var generatedOrderFromStandingOrderQuery = 
        string.Format("select o.Id, o.ShipDate, od.ProductId, p.Name " + 
        " from vwOrderDetail od " + 
        " join vwProducts p on od.ProductID = p.Id " +
        " join vwOrders o on o.Id = od.OrderId " + 
        " where od.ProductId in(select ProductId from vwStandingOrProd sop where sop.StandingOrderID = {0}) " + 
        " and o.ShipToId in (select ShipToId from vwStandingOrders so where so.Id = {0}) " + 
        " order by o.ShipDate desc ", standingOrderId);

    var ordersForProductsTable = da.GetDataTable(generatedOrderFromStandingOrderQuery);

    foreach(DataRow orderForProductRow in ordersForProductsTable.Rows)
        Console.WriteLine(string.Format("ShipDate: {0} | OrderId: {1} |  | ProductId: {2} | ProductName: {3}", orderForProductRow["ShipDate"], orderForProductRow["Id"], orderForProductRow["ProductId"], orderForProductRow["Name"]));

    Console.WriteLine("--- Decision ---");

    if (Console.ReadLine() == "fix") {
        var orderId = InputUtilities.GetUserInputInteger("Enter OrderId to use for Payment:");
        var standingOrderGe = app.GetEntityObject("Standing Orders", standingOrderId);
        var scheduleSubType = standingOrderGe.SubTypes["StandingOrSchedule"];
        
        var scheduleToFixGe = (AptifyGenericEntityBase) scheduleSubType[0];
        scheduleToFixGe.SetValue("IsComplete", 1);
        scheduleToFixGe.SetValue("Custom", 1);
        scheduleToFixGe.SetValue("OrderID", orderId);
        scheduleToFixGe.SetValue("PaymentID", da.ExecuteScalar("select top 1 PaymentID from vwPaymentDetail where orderid = " + orderId));
        
        var newScheduleGe = (AptifyGenericEntityBase) scheduleSubType.Add();
        newScheduleGe.SetValue("ScheduledDate", lastBillingDate);

        Console.WriteLine("--- Saving Standing Order ---");

        var errorString = string.Empty;
        if (!standingOrderGe.Save(false, ref errorString))
            Console.WriteLine(string.Format("Error Saving Standing Order: {0}", errorString));

        Console.WriteLine("--- Saved Standing Order ---");
    }
} 
else 
    Console.WriteLine("Standing Order has more than one row of Schedule.");

Console.WriteLine("--- Complete ---");