# CRUD-ASPNET-JWT


## Peticiones 
- GET : https://apitokenrada.azurewebsites.net/producto
- GET POR ID : https://apitokenrada.azurewebsites.net/producto/{id}
- POST : https://apitokenrada.azurewebsites.net/producto
- PUT : https://apitokenrada.azurewebsites.net/producto/{id}
- DELETE : https://apitokenrada.azurewebsites.net/producto/{id}

### Modelo
```
{
    "id" : int,
    "nombre": string
}
```


Se requiere loguear para poder funcionar en:
``` 
https://apitokenrada.azurewebsites.net/accounts/login
```
Con un body en formato JSON 
```
{
    "username": "string", //Utilizar dichas credenciales para obtener Token.
    "password": "string"
}
```
El token resultante, se deberá pasar como Header Authorization Bearer.

También puede entrar a: https://apitokenrada.azurewebsites.net/swagger/index.html

**OJO**: El token introducido en Swagger, deberá llevar el prefijo Bearer.

