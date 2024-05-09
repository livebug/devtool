

update EntityConfigurationValues set Value = datetime('now')
where Key='WidgetOptions:EndpointId'

;


select * from EntityConfigurationValues
;