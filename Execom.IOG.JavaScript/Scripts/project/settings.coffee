

namespace = (target, name, block) ->
  [target, name, block] = [(if typeof exports isnt 'undefined' then exports else window), arguments...] if arguments.length < 3
  top    = target
  target = target[item] or= {} for item in name.split '.'
  block target, top

namespace "execom.iog", (exports) ->
  exports.name = "execom.iog"
  
  $(document).ready(() -> 
    $.connection.hub.logging = true

    $.connection.sendEvent = (args) ->
      console.log(args)

    $.connection.eventsHub.sendEvent = (args) ->
      console.log(args)
      argsObj = JSON.parse(args)
      exports.events.trigger(argsObj.Subscription.SubscriptionId, argsObj)

    $.connection.hub.start(transport: ['webSockets','longPolling'])
      .done(() ->
        console.log("Now connected! Client ID: " +
          $.connection.hub.id))
      .fail(() ->
        console.log("Could not Connect!"))
  )

namespace execom.iog.name, (exports) ->
  exports.IOGSettings = 
    ObjectCacheMinimumCount: 50000
    ObjectCacheMaximumCount: 100000