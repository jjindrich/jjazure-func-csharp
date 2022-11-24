param cosmosName string = 'jjazcosmos'
param cosmosDbName string = 'jjdb'
param functionAppName string = 'jjazfunc'
param location string = resourceGroup().location

///////////////////////////////
// Cosmos DB
///////////////////////////////
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

/////////////////////////////////
// Azure Function
/////////////////////////////////
var applicationInsightsName = functionAppName
var storageAccountName = '${functionAppName}${uniqueString(resourceGroup().id)}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'Storage'
}

resource hostingPlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: functionAppName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: hostingPlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNET|6.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
        }
      ]
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}
