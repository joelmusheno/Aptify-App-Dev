using Aptify.Framework.Application;
using Aptify.Framework.DataServices;
using Aptify.Framework.ExceptionManagement;
using Aptify.Framework.BusinessLogic;
using Aptify.Framework.BusinessLogic.GenericEntity;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using Aptify.Applications.OrderEntry;

var aptifyPass = Environment.GetEnvironmentVariable("StgAptifyDbPassword");
var uc = new UserCredentials("stgaptifydb.ohiobar.org", "Aptify", "Aptify", false, -1, "sa", aptifyPass, null, false, -1, true);
var da = new DataAction(uc);

Console.WriteLine("Booting Aptify");
var app = new AptifyApplication(uc);
Console.WriteLine("Aptify Loaded");
 
var productId = 26274;
var attendeeId = 43291;

var orderGe = app.GetEntityObject("Orders", -1);
orderGe.SetValue("ShipToID", attendeeId);
orderGe.SetValue("EmployeeID", 1);

Console.WriteLine("Creating new Order");
var orderProper = (OrdersEntity)orderGe;

Console.WriteLine("Adding Order Line");
var orderLines = orderProper.AddProduct(productId);

foreach (var orderLine in orderLines)
{
    if (orderLine.ProductID == productId && orderLine.ExtendedOrderDetailEntity != null)
    {
        if (orderLine.ExtendedOrderDetailEntity.EntityName == "Class Registrations" &&
            Convert.ToInt32(orderLine.ExtendedOrderDetailEntity.GetValue("StudentID")) !=
            attendeeId)
        {
            // orderLine.ExtendedOrderDetailEntity
            //     .SetValue("ClassID", GetClassIdFromProductId(productId));
            orderLine.ExtendedOrderDetailEntity
                .SetValue("StudentID", attendeeId);
            orderLine.ExtendedOrderDetailEntity
                .SetValue("Status", "Registered");
        }
        
        if (orderLine.ExtendedOrderDetailEntity.EntityName == "OrderMeetingDetail" &&
            Convert.ToInt32(orderLine.ExtendedOrderDetailEntity.GetValue("AttendeeID")) !=
            attendeeId)
        {
            orderLine.ExtendedOrderDetailEntity
                .SetValue("AttendeeID", attendeeId);
            orderLine.ExtendedOrderDetailEntity
                .SetValue("ProductID", productId);
            orderLine.ExtendedOrderDetailEntity
                .SetValue("RegistrationType", "Pre-Registration");
        }
    }
}

orderProper.SetAddValue("InitialPaymentAmount", orderProper.GrandTotal);
orderProper.SetAddValue("PayTypeID", 42 ); //visa - from epi
orderProper.SetAddValue("CCAccountNumber", "4111111111111111");
orderProper.SetAddValue("CCExpireDate", "1/1/21");
orderProper.SetAddValue("CCSecurityNumber", "123");

var paymentInformationField = orderProper.Fields["PaymentInformationID"];
var paymentInformationGe = paymentInformationField.EmbeddedObject;

var authCode = "010101";
var referenceTransactionNumber = "A10FAA21289B";
var referenceExpiration = new DateTime().AddYears(1);

paymentInformationGe.SetValue("CCAuthCode", authCode);
paymentInformationGe.SetValue("ReferenceTransactionNumber", referenceTransactionNumber);
paymentInformationGe.SetValue("ReferenceExpiration", referenceExpiration);


System.Console.WriteLine("Got Here");




var errorString = string.Empty;
if (!((AptifyGenericEntityBase)orderProper).Save(false, ref errorString))
    throw new Exception(errorString);

var paymentTable = da.GetDataTable($"select PaymentId from vwPaymentDetail pd where pd.OrderID = {orderProper.ID}");
foreach (DataRow dr in paymentTable.Rows) {
    
    var paymentId = Convert.ToInt32(dr["PaymentId"]);
    var paymentGe = app.GetEntityObject("Payments", paymentId);
    
    System.Console.WriteLine($"Updating PaymentId {paymentId}");

    var paGe = (AptifyGenericEntityBase)paymentGe.SubTypes["PaymentAuthorizations"].Add();

    paGe.SetValue("AuthorizationDate", DateTime.Now.ToString());
    paGe.SetValue("AuthorizationCode", authCode);
    paGe.SetValue("AuthorizationType", "Authorize");
    paGe.SetValue("ReferenceNumber", referenceTransactionNumber);
    paGe.SetValue("Amount", orderProper.GrandTotal);
    paGe.SetValue("ProcessorRefNumber", "A10FAA21289B-PRN");
    paGe.SetValue("MerchantAccountID", 1);

    if (paymentGe.Save(false, ref errorString)) 
        throw new Exception(errorString);
}




Console.WriteLine($"Created Order {orderProper.ID}");
