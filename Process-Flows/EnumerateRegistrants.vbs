Dim app As Aptify.Framework.Application.AptifyApplication
app = New Aptify.Framework.Application.AptifyApplication(oDataAction.UserCredentials)

Dim orderId As Integer
orderId = System.Convert.ToInt32(oProperties.Item("orderId"))

Dim orderGe As Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase
orderGe = CType(oProperties.Item("orderGe"), Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase)

If (orderGe Is Nothing) Then
	orderGe = app.GetEntityObject("Orders", orderId)
End If

Dim orderLinesSubtypeBase As Aptify.Framework.BusinessLogic.GenericEntity.AptifySubTypeBase
orderLinesSubtypeBase = orderGe.SubTypes("OrderLines")

Dim orderLinesEnumerable As System.Collections.IEnumerable
orderLinesEnumerable = CType(orderLinesSubtypeBase, System.Collections.IEnumerable)

Dim orderLinesEnumerator As System.Collections.IEnumerator
orderLinesEnumerator = orderLinesEnumerable.GetEnumerator

While (orderLinesEnumerator.MoveNext)
	Dim orderLine As Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase
	orderLine = CType(orderLinesEnumerator.Current, Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase)
	
	Dim productGe As Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase
	productGe = app.GetEntityObject("Products", System.Convert.ToInt64(orderLine.GetValue("ProductID")))
	
	Dim productTypeGe As Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase
	productTypeGe = app.GetEntityObject("Product Types", System.Convert.ToInt64(productGe.GetValue("ProductTypeID")))
	
	Dim orderLineExtendedEntityName As String
	orderLineExtendedEntityName = System.Convert.ToString(productTypeGe.GetValue("ExtendedOrderDetailEntity"))
	
	If Not String.IsNullOrEmpty(orderLineExtendedEntityName) Then
		
		Dim extendedOrderDetailEntityId As Long 
		extendedOrderDetailEntityId = System.Convert.ToInt64(orderLine.GetValue("ExtendedAttributeID"))
		
		Dim orderLineExtendedEntity As Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase
		orderLineExtendedEntity = app.GetEntityObject(orderLineExtendedEntityName, extendedOrderDetailEntityId)
		
		Dim registrantId As Long
			
		If (orderLineExtendedEntityName.Equals("Class Registrations")) Then
			registrantId = System.Convert.ToInt64(orderLineExtendedEntity.GetValue("StudentID"))
		ElseIf (orderLineExtendedEntityName.Equals("OrderMeetingDetail")) Then
			registrantId = System.Convert.ToInt64(orderLineextendedEntity.GetValue("AttendeeID"))
		End If
		
		
		' TODO - do what is necessary with person in question
		' System.Windows.Forms.MessageBox.Show(registrantId)
		
	End If
	
	
End While	
