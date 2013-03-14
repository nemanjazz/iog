namespace execom.iog.name, (exports) ->
  class exports.AutoOverrideMergeRuleProvider
    IsConcurrent: (typeId) ->
      return true
    IsMemberOverride: (typeId, memberId) ->
      return true

    IsStaticConcurrency: (typeId) ->
      return true