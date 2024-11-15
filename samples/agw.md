# Azure Application Gateway

When we have a ACI with a DNS label = kvo181

```bash
az network application-gateway create --name agwkvo181 --location westeurope -g rg-containers --capacity 1 --public-ip-address agwPIP --vnet-name gwVnet --subnet gwSubnet --servers kvo181.westeurope.azurecontainer.io --sku WAF_v2 --http-settings-port 8080 --priority 100 --waf-policy myPolicy
```
