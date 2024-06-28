# RenameFilesAzure

Este projeto demonstra como listar, renomear e mover blobs no Azure Blob Storage, além de atualizar os registros correspondentes em um banco de dados PostgreSQL.

## Visão Geral

A aplicação realiza as seguintes tarefas:
1. Conecta-se ao Azure Blob Storage.
2. Conecta-se a um banco de dados PostgreSQL.
3. Busca registros que contêm URLs de arquivos.
4. Renomeia e move blobs no Azure Blob Storage.
5. Atualiza os registros no banco de dados PostgreSQL com os novos URLs dos arquivos.

## Requisitos

- .NET SDK (versão 6.0 ou superior)
- Conta do Azure com Blob Storage configurado
- Banco de dados PostgreSQL

## Configuração

### Passos para Configuração

1. **Clone o repositório:**

    ```bash
    git clone https://github.com/usuario/RenameFilesAzure.git
    cd RenameFilesAzure
    ```

2. **Atualize as configurações no arquivo `Program.cs`:**

   Edite o arquivo `Program.cs` e substitua os placeholders com suas credenciais e informações reais:

    ```csharp
    private const string AzureBlobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=SEU_ACCOUNT_NAME;AccountKey=SEU_ACCOUNT_KEY;EndpointSuffix=core.windows.net";
    private const string BlobContainerName = "SEU_CONTAINER_NAME";
    private const string PostgresConnectionString = "Host=SEU_HOST;Database=SEU_DATABASE;Port=5432;Username=SEU_USERNAME;Password=SEU_PASSWORD;SslMode=Require;Trust Server Certificate=true";
    ```

3. **Compile o projeto:**

    ```bash
    dotnet build
    ```

4. **Execute a aplicação:**

    ```bash
    dotnet run
    ```

## Estrutura do Projeto

- **Program.cs**: Contém a lógica principal para conectar-se ao Azure Blob Storage e ao banco de dados PostgreSQL, listar blobs, renomeá-los, movê-los e atualizar os registros no banco de dados.

## Notas Adicionais

### Configurações do Azure Blob Storage

- Certifique-se de que seu `AzureBlobStorageConnectionString` está corretamente configurado com as credenciais da sua conta do Azure.
- Certifique-se de que o `BlobContainerName` está correto e o container existe.

### Configurações do Banco de Dados PostgreSQL

- Certifique-se de que seu `PostgresConnectionString` está corretamente configurado com as credenciais do seu banco de dados PostgreSQL.
- Certifique-se de que a tabela e as colunas especificadas na consulta SQL existem no banco de dados.

### Personalizações

- A função `GenerateNewBlobName` pode ser personalizada para ajustar a lógica de renomeação conforme necessário.

## Contribuição

Sinta-se à vontade para contribuir com este projeto. Você pode abrir issues e pull requests para melhorias e correções.

## Licença

Este projeto é licenciado sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

