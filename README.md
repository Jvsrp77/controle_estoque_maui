# Tech Stock Pro — Controle de Estoque

Aplicação multiplataforma de controle de estoque de eletrônicos, desenvolvida em **C# com .NET MAUI**, com persistência em MySQL. Roda a partir do mesmo código-base em Android, iOS, macOS e Windows.

Projeto acadêmico de Engenharia da Computação (Univap), com foco em modelagem relacional e integridade transacional.

<!-- Adicione um print da aba Estoque rodando no Windows e descomente a linha abaixo:
![Tela de controle de estoque do Tech Stock Pro](screenshot.jpg)
-->

## O que o projeto faz

A aplicação é organizada em três abas (`Shell` com `TabBar`):

- **Estoque** — painel com total de unidades em estoque e contagem de produtos esgotados, cadastro de novos produtos (nome, preço, quantidade, categoria e fornecedor) e baixa de estoque por venda. A lista de produtos é carregada do banco e exibida com ID, nome, preço formatado em reais e saldo.
- **Fornecedores** — cadastro e listagem de fornecedores (razão social e CNPJ).
- **Categorias** — cadastro e listagem de categorias de produto.

### Baixa de estoque transacional

A operação de retirada é o núcleo do projeto. Ao confirmar uma venda, `PedidoVendaDAO.RegistrarVenda` abre uma **transação MySQL** e executa três escritas:

1. Insere o registro de faturamento em `Pedidos_Vendas` e recupera o ID gerado via `LAST_INSERT_ID()`.
2. Insere o item correspondente em `Itens_Pedido`, associando produto, quantidade e preço unitário praticados no momento da venda.
3. Decrementa `quantidade_estoque` na tabela `Produto`.

Se qualquer etapa falhar, a transação sofre **rollback** — não existe venda gravada sem baixa de estoque, nem baixa sem venda. A tentativa de retirar mais do que existe é bloqueada com aviso ao usuário; a checagem acontece na tela, antes da chamada ao DAO.

O decremento é feito pelo próprio banco (`quantidade_estoque = quantidade_estoque - @qtd`), e não lido para memória e regravado — duas vendas concorrentes não se sobrescrevem.

## Stack

- **.NET 10** / **C#**
- **.NET MAUI** — interface declarativa em XAML, navegação via `Shell`
- **MySQL** — acesso via `MySql.Data` (ADO.NET), com *prepared statements* em todas as queries
- **Padrão DAO** — acesso a dados isolado em `DAO/`, com contrato em `Interfaces/IProdutoDAO.cs`

**Plataformas-alvo:** Android (API 21+), iOS 15+, Mac Catalyst 15+ e Windows 10 (build 17763+).

## Estrutura

```
controle_estoque_maui/
├── models/
│   └── Produto.cs              Entidade de produto
├── Interfaces/
│   └── IProdutoDAO.cs          Contrato de persistência de produto
├── DAO/
│   ├── ProdutoDAO.cs           CRUD de produto + listagem com saldo
│   └── PedidoVendaDAO.cs       Registro de venda transacional
├── Views/
│   ├── EstoquePage.xaml        Painel, cadastro e baixa de estoque
│   ├── FornecedorPage.xaml     Cadastro de fornecedores
│   └── CategoriaPage.xaml      Cadastro de categorias
├── AppShell.xaml               Navegação por abas
└── MauiProgram.cs              Composição da aplicação
```

## Banco de dados

O projeto espera um banco MySQL chamado `controle_estoque_eletronicos` com cinco tabelas. Script de criação:

```sql
CREATE DATABASE IF NOT EXISTS controle_estoque_eletronicos;
USE controle_estoque_eletronicos;

CREATE TABLE Categorias (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    nome         VARCHAR(100) NOT NULL
);

CREATE TABLE Fornecedores (
    id_fornecedor INT AUTO_INCREMENT PRIMARY KEY,
    nome_empresa  VARCHAR(150) NOT NULL,
    cnpj          VARCHAR(18)  NOT NULL
);

CREATE TABLE Produto (
    id_produto         INT AUTO_INCREMENT PRIMARY KEY,
    nome               VARCHAR(150)   NOT NULL,
    preco              DECIMAL(10,2)  NOT NULL,
    quantidade_estoque INT            NOT NULL DEFAULT 0,
    id_categoria       INT            NOT NULL,
    id_fornecedor      INT            NOT NULL,
    CONSTRAINT fk_produto_categoria
        FOREIGN KEY (id_categoria)  REFERENCES Categorias(id_categoria),
    CONSTRAINT fk_produto_fornecedor
        FOREIGN KEY (id_fornecedor) REFERENCES Fornecedores(id_fornecedor)
);

CREATE TABLE Pedidos_Vendas (
    id_pedido   INT AUTO_INCREMENT PRIMARY KEY,
    data_venda  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    valor_total DECIMAL(10,2) NOT NULL
);

CREATE TABLE Itens_Pedido (
    id_item        INT AUTO_INCREMENT PRIMARY KEY,
    id_pedido      INT           NOT NULL,
    id_produto     INT           NOT NULL,
    quantidade     INT           NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_item_pedido
        FOREIGN KEY (id_pedido)  REFERENCES Pedidos_Vendas(id_pedido),
    CONSTRAINT fk_item_produto
        FOREIGN KEY (id_produto) REFERENCES Produto(id_produto)
);
```

O relacionamento `Pedidos_Vendas` ↔ `Itens_Pedido` ↔ `Produto` é o que permite registrar uma venda com múltiplos itens preservando o preço histórico de cada um.

## Como rodar

**Pré-requisitos**

- [.NET 10 SDK](https://dotnet.microsoft.com/download) com a workload MAUI instalada:
  ```bash
  dotnet workload install maui
  ```
- MySQL em execução (o projeto está configurado para a porta **3307**)
- Visual Studio 2022+ com a carga de trabalho *Desenvolvimento para dispositivos móveis com .NET* (opcional, mas recomendado para depurar em Android/Windows)

**Passos**

1. Crie o banco executando o script da seção anterior.
2. Ajuste a string de conexão, se necessário. Ela está declarada em `DAO/ProdutoDAO.cs`, `DAO/PedidoVendaDAO.cs`, `Views/CategoriaPage.xaml.cs` e `Views/FornecedorPage.xaml.cs`:
   ```csharp
   "Server=localhost;Port=3307;Database=controle_estoque_eletronicos;Uid=root;Pwd=;"
   ```
3. Restaure e execute:
   ```bash
   dotnet restore
   dotnet build
   dotnet run --framework net10.0-windows10.0.19041.0
   ```
   Para Android, use `--framework net10.0-android` com um emulador ativo.
4. Cadastre ao menos **uma categoria e um fornecedor** antes de cadastrar produtos — o cadastro de produto exige os IDs de ambos e falha por restrição de chave estrangeira sem eles.

## Limitações conhecidas

Pontos deliberadamente fora do escopo do trabalho acadêmico, registrados aqui por transparência:

- **String de conexão em código e duplicada** em quatro arquivos. O caminho correto é centralizar em configuração externa (`appsettings.json` ou *secrets*) e injetar via DI.
- **Acesso a dados síncrono** na thread de UI. As operações são rápidas em base local, mas o padrão adequado em MAUI é `async`/`await` com os métodos `...Async` do conector.
- **DAOs instanciados diretamente** nas páginas, em vez de registrados no contêiner de injeção de dependência do `MauiProgram`.
- **Cadastro por ID digitado** — categoria e fornecedor são informados como número. Um `Picker` populado do banco seria o comportamento esperado em produção.
- **CRUD parcial na interface** — `Alterar` e `Excluir` existem em `ProdutoDAO` e na interface `IProdutoDAO`, mas ainda não têm ação correspondente na tela.
- **A validação de saldo está na interface, não na transação.** `EstoquePage` confere o estoque antes de chamar `RegistrarVenda`, mas checagem e escrita não são atômicas: duas vendas simultâneas do mesmo produto podem passar ambas pela conferência e levar o saldo a negativo. Em aplicação de um usuário isso não ocorre. O correto seria `UPDATE ... WHERE quantidade_estoque >= @qtd` e tratar zero linhas afetadas como saldo insuficiente.
- **Sem camada de testes.**

## Licença

[MIT](LICENSE)
