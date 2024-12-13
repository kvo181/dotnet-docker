# Azure Application Gateway

When we have a ACI with a DNS label = kvo181

```bash
az network application-gateway create --name agwkvo181 --location westeurope -g rg-containers --capacity 1 --public-ip-address agwPIP --vnet-name gwVnet --subnet gwSubnet --servers kvo181.westeurope.azurecontainer.io --sku WAF_v2 --http-settings-port 8080 --priority 100 --waf-policy myPolicy

az network application-gateway rewrite-rule set create --name ForwardedForRuleSet --gateway-name agwkvo181 --resource-group rg-containers

az network application-gateway rewrite-rule create -g rg-containers --gateway-name agwkvo181 --rule-set-name ForwardedForRuleSet -n rule1 --sequence 100 --request-headers "X-Forwarded-For={var_client_ip}"

az network application-gateway rule update -g rg-containers --gateway-name agwkvo181 -n rule1 --rewrite-rule-set ForwardedForRuleSet

az network application-gateway rewrite-rule list --gateway-name agwkvo181 --resource-group rg-containers --rule-set-name ruleset1

$id =$(az identity show -g rg-containers -n msi-func-kvo181-081e --query 'id' -o tsv)
az network application-gateway identity assign -g rg-containers --gateway-name agwkvo181 --identity $id


az keyvault certificate show --vault-name kv-kvo181 --name kvo181-xyz-1 --query 'sid' -o tsv
$secretId=$('https://kv-kvo181.vault.azure.net/secrets/kvo181-xyz-1/')
az network application-gateway ssl-cert create -g rg-containers --gateway-name agwkvo181 --name kvo181-xyz --key-vault-secret-id $secretId

```

```powershell
$requestHeaderConfiguration = New-AzApplicationGatewayRewriteRuleHeaderConfiguration -HeaderName "X-Forwarded-For" -HeaderValue "{var_client_ip}"
#$responseHeaderConfiguration = New-AzApplicationGatewayRewriteRuleHeaderConfiguration -HeaderName "Strict-Transport-Security" -HeaderValue "max-age=31536000"
$actionSet = New-AzApplicationGatewayRewriteRuleActionSet -RequestHeaderConfiguration $requestHeaderConfiguration
#$actionSet = New-AzApplicationGatewayRewriteRuleActionSet -RequestHeaderConfiguration $requestHeaderConfiguration -ResponseHeaderConfiguration $responseHeaderConfiguration
$rewriteRule = New-AzApplicationGatewayRewriteRule -Name rewriteRule1 -ActionSet $actionSet
$rewriteRuleSet = New-AzApplicationGatewayRewriteRuleSet -Name ruleSet1 -RewriteRule $rewriteRule

$AppGw = Get-AzApplicationGateway -Name "agwkvo181" -ResourceGroupName "rg-containers"
$AppGw = Add-AzApplicationGatewayRewriteRuleSet -ApplicationGateway $AppGw -Name "ruleset1" -RewriteRule $rewriteRule
Set-AzApplicationGateway -ApplicationGateway $AppGw
```


