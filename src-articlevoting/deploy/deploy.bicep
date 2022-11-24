param cosmosName string = 'jjazcosmos'
param cosmosDbName string = 'jjazcosmosdb'
param location string = resourceGroup().location

resource cosmos 'Microsoft.DocumentDB/databaseAccounts@2022-08-15' = {
  name: cosmosName
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    enableMultipleWriteLocations: false
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}
resource database 'Microsoft.DocumentDB/databaseAccounts/apis/databases@2016-03-31' = {
  name: '${cosmos.name}/sql/${cosmosDbName}'
  properties: {
    options: {}
    resource: {
      id: cosmosDbName
    }
  }
}
resource containerArticles 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-08-15' = {
  name: '${cosmos.name}/${cosmosDbName}/articles'
  properties: {
    resource: {
      id: 'articles'
      partitionKey: {
        paths: [
          '/articleid'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'Consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
    }
  }
  dependsOn: [
    database
  ]
}
resource containerVotes 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2022-08-15' = {
  name: '${cosmos.name}/${cosmosDbName}/votes'
  properties: {
    resource: {
      id: 'votes'
      partitionKey: {
        paths: [
          '/voteid'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'Consistent'
        automatic: true
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
    }
  }
  dependsOn: [
    database
  ]
}
