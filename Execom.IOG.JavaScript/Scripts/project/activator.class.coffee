
#this class will replace Activator class from .NET library
class window.ActivatorClass
  constructor: () ->

  CreateInstance: (type, facade, instanceId, readOnly) ->
    #TODO here I didn't finished implementation

Activator = new ActivatorClass()