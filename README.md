# Report Adhoc para Banco de Dados PostgreSQL usando Entity Framework Core

## Descrição

Este projeto consiste em um relatório Adhoc utilizando framework ORM Entity Framework Core com banco de dados PostgreSQL. Foi desenvolvido como parte de um projeto da disciplina de Banco de Dados II na Universidade Federal de Itajubá - UNIFEI. A Api utilizada foi a [Perenual](https://perenual.com/docs/api)

## Instalação

Para executar este projeto, é necessário ter o ambiente .NET configurado e as seguintes bibliotecas:

- **RestSharp**
- **Newtonsoft.Json**
- **Npgsql** (driver de conexão para PostgreSQL)
- **Entity Framework Core**
- **Angular (Requer NodeJs versão 16)**

## Para executar


```sh
cd ClientApp                                           #Entrar em ClientApp pelo terminal
.../ClientApp/npm i                                    #(ou ng i, caso não funcione)
.../ClientApp/ng build --configuration production
cd AdHocTest                                           #Entrar no AdHocTest(projeto asp net) pelo terminal
.../AdHocTest/dotnet run
```
- Será fornecida a porta com o endereço localhost para a conexão

## Grupo

Este projeto foi desenvolvido por:

- **Danubia Borges - 2019018489**
- **Caio Miranda Caetano Antunes - 2021024231**
- **Giulia Garcia Castro Rodrigues - 2020000191**
- **Vinicius Vieira Mota - 2020027838**
- **Vinicius Ribeiro Marques - 2020019184**

