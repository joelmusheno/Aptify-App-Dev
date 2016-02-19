oResultCode.Value = "Failed"

Dim app As Aptify.Framework.Application.AptifyApplication
app = New Aptify.Framework.Application.AptifyApplication(oDataAction.UserCredentials)

Dim committeeTermId As Integer 
committeeTermId = System.convert.ToInt32(oProperties.GetProperty("committeeTermId"))

' Gather process flows to execute (in this case from properties)
Dim sendPersonPfId As Integer
sendPersonPfId = System.convert.ToInt32(oProperties.GetProperty("sendPersonPfId"))
''''


' Iteration of subtypes to gather process flow parameters
Dim committeeTermGe As Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase
committeeTermGe = app.GetEntityObject("Committee Terms", committeeTermId)

Dim committeeTermMembersSubtypeBase As Aptify.Framework.BusinessLogic.GenericEntity.AptifySubTypeBase
committeeTermMembersSubtypeBase = committeeTermGe.SubTypes("CommitteeTermMembers")

Dim committeeTermMembersEnumerable As System.Collections.IEnumerable
committeeTermMembersEnumerable = CType(committeeTermMembersSubtypeBase, System.Collections.IEnumerable)

Dim committeeTermMembersEnumerator As System.Collections.IEnumerator
committeeTermMembersEnumerator = committeeTermMembersEnumerable.GetEnumerator

While (committeeTermMembersEnumerator.MoveNext)
	Dim ctmGe As Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase
	ctmGe = CType(committeeTermMembersEnumerator.Current, Aptify.Framework.BusinessLogic.GenericEntity.AptifyGenericEntityBase)
	
	Dim personId As Integer 
	personId = SYstem.convert.ToInt32(ctmGe.GetValue("MemberID"))
	
	If personId <> 0 Then
		
		' context for process flows to run 
		Dim pfContext As Aptify.Framework.Application.AptifyContext
		pfContext = New Aptify.Framework.Application.AptifyContext
		
		' input properties for process flow
		pfContext.Properties.AddProperty("EntityId", 1006)
		pfContext.Properties.AddProperty("RecordId", personId)
		
		' execute the process flow from the context of the current
		Dim pfEngine As Aptify.Framework.BusinessLogic.ProcessPipeline.ProcessFlowEngine
		pfEngine.ExecuteProcessFlow(app, sendPersonPfId, pfContext)
	
	End If
		

End While

oResultCode.Value = "Success"


