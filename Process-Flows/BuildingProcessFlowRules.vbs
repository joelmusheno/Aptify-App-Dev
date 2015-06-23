Dim app As Aptify.Framework.Application.AptifyApplication
app = New Aptify.Framework.Application.AptifyApplication(oDataAction.UserCredentials)

Dim orderId As Integer
orderId = System.Convert.ToInt32(oProperties.Item("orderId"))

Dim orderGe As Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase
orderGe = CType(oProperties.Item("orderGe"), Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase)

If (orderGe Is Nothing) Then
	orderGe = app.GetEntityObject("Orders", orderId)
End If

' TODO - now consume entity object