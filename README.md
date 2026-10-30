# Tech Stock Pro — Controle de Estoque

Aplicação multiplataforma de controle de estoque de eletrônicos, desenvolvida em **C# com .NET MAUI**, com persistência em MySQL. Roda a partir do mesmo código-base em Android, iOS, macOS e Windows.

Projeto acadêmico de Engenharia da Computação (Univap), com foco em modelagem relacional e integridade transacional.

<!-- Adicione um print da aba Estoque rodando no Windows e descomente a linha abaixo:
![Tela de controle de estoque do Tech Stock Pro](screenshot.jpg)
-->

## O que o projeto faz

A aplicação é organizada em três abas (`Shell` com `TabBar`):

- **Estoque** — painel com total de unidades e contagem de esgotados; cadastro, **edição e exclusão** de
  produtos, com categoria e fornecedor escolhidos em lista suspensa; e baixa de estoque por venda.
  Produto já vendido não pode ser excluído: a chave estrangeira de `Itens_Pedido` protege o histórico.
- **Fornecedores** — cadastro e listagem de fornecedores (razão social e CNPJ).
- **Categorias** — cadastro e listagem de categorias de produto.

### Baixa de estoque transacional

A operação de retirada é o núcleo do projeto. Ao confirmar uma venda,
`PedidoVendaDAO.RegistrarVendaAsync` abre uma **transação** e executa três escritas:

1. Decrementa `quantidade_estoque` em `Produto`, **condicionado a haver saldo**
   (`WHERE quantidade_estoque >= @qtd`).
2. Insere o registro de faturamento em `Pedidos_Vendas` e recupera o ID gerado.
3. Insere o item em `Itens_Pedido`, com o preço praticado no momento da venda.

Se qualquer etapa falhar, a transação sofre **rollback** — não existe venda
gravada sem baixa de estoque, nem baixa sem venda.

**A ordem não é acidental.** A baixa vem primeiro por dois motivos:

- É a única etapa que pode ser *recusada*. Com ela na frente, uma venda sem
  saldo não chega a escrever nada que depois precise ser desfeito.
- É a linha disputada por vendas concorrentes. Todas as transações pegam a
  trava da mesma linha logo de início, na ordem em que chegam — o que elimina
  o deadlock que aparecia quando a baixa era a última etapa.

A conferência de saldo **faz parte do próprio `UPDATE`**, e não de uma consulta
anterior. Conferir antes deixaria uma janela entre a leitura e a escrita em que
outra venda poderia consumir o mesmo saldo. Zero linhas afetadas significa
"não havia saldo" (ou o produto sumiu), e o método devolve um
`ResultadoVenda` em vez de lançar exceção.

A subtração também é feita pelo banco (`quantidade_estoque = quantidade_estoque - @qtd`),
e não lida para a memória e regravada: duas vendas simultâneas se sobrescreveriam.

## Stack

- **.NET 10** / **C#**
- **.NET MAUI** — interface declarativa em XAML, navegação via `Shell`
- **MySQL / MariaDB** — acesso via `MySql.Data` (ADO.NET), com *prepared statements*
  em todas as queries e acesso totalmente assíncrono
- **Injeção de dependência** — configuração e DAOs registrados em `MauiProgram`
  e entregues às páginas pelo construtor
- **xUnit** — testes de integração contra um banco descartável

**Plataformas-alvo:** Android (API 21+), iOS 15+, Mac Catalyst 15+ e Windows 10 (build 17763+).

## Estrutura

A solução tem três projetos. O domínio e o acesso a dados vivem fora do projeto
MAUI — é o que permite testá-los sem subir a interface.

```
controle_estoque_maui.Core/        biblioteca net10.0, sem dependência de UI
├── appsettings.json               string de conexão (recurso embutido)
├── Config/AppConfig.cs            leitura da configuração
├── models/                        Produto, Categoria, Fornecedor, ResultadoVenda
├── Interfaces/                    contratos dos DAOs
└── DAO/
    ├── ProdutoDAO.cs              CRUD de produto
    ├── CategoriaDAO.cs
    ├── FornecedorDAO.cs
    └── PedidoVendaDAO.cs          venda transacional

controle_estoque_maui/             aplicação MAUI
├── MauiProgram.cs                 registro das dependências
├── AppShell.xaml(.cs)             abas, com as páginas vindas do contêiner
└── Views/
    ├── EstoquePage                painel, cadastro, edição, exclusão e baixa
    ├── FornecedorPage
    └── CategoriaPage

controle_estoque_maui.Tests/       xUnit
├── BancoDeTesteFixture.cs         cria e derruba um banco por execução
└── RegistrarVendaTests.cs
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

- [.NET 10 SDK](https://dotnet.microsoft.com/download) com a workload MAUI:
  ```bash
  dotnet workload install maui
  ```
- MySQL ou MariaDB em execução (o padrão aponta para a porta **3307**)

**Passos**

1. Crie o banco e os dados de demonstração. **Declare o charset**, senão os
   acentos entram corrompidos:
   ```bash
   mysql -h 127.0.0.1 -P 3307 -u root --default-character-set=utf8mb4 < seed_demo.sql
   ```
2. Se o seu banco não estiver em `localhost:3307`, aponte para o seu sem
   recompilar:
   ```bash
   set ESTOQUE_CONNECTIONSTRING=Server=localhost;Port=3306;Database=controle_estoque_eletronicos;Uid=root;Pwd=senha;
   ```
   Sem a variável, vale o valor de `controle_estoque_maui.Core/appsettings.json`.
3. Execute:
   ```bash
   dotnet build
   dotnet run --project controle_estoque_maui -f net10.0-windows10.0.19041.0
   ```
   Para Android, use `-f net10.0-android` com um emulador ativo.

Categorias e fornecedores aparecem em listas suspensas no cadastro de produto —
não é preciso decorar IDs.

## Testes

```bash
dotnet test
```

São testes de **integração**: cada execução cria um banco descartável
(`estoque_teste_<guid>`), aplica o schema e o derruba no fim. Nunca tocam o
banco de desenvolvimento. Exigem um MySQL/MariaDB acessível — o endereço vem de
`ESTOQUE_TESTE_SERVIDOR`, com `localhost:3307` como padrão.

Cinco testes cobrem a venda transacional:

| Teste | O que garante |
|---|---|
| Venda com saldo | Baixa o estoque e grava pedido e item |
| Venda sem saldo | Devolve `SaldoInsuficiente` e **não deixa resíduo** |
| Produto inexistente | Devolve `ProdutoNaoEncontrado` sem lançar exceção |
| Venda do saldo exato | Aceita a última unidade e recusa a seguinte |
| **Vendas simultâneas** | Dez vendas disputando estoque de 6: exatamente 6 passam, 4 são recusadas, saldo final zero — nunca negativo |

O último é o que justifica a conferência estar dentro do `UPDATE`. Ele falhava
com deadlock quando a baixa era a última das três escritas; foi o que motivou
a reordenação descrita acima.

## Limitações conhecidas

Registradas por transparência — são os pontos que eu trataria antes de
considerar isto pronto para produção:

- **Um item por pedido.** O schema suporta pedido com vários itens
  (`Pedidos_Vendas` ↔ `Itens_Pedido`), mas `RegistrarVendaAsync` grava um
  produto por vez. Vender três itens cria três pedidos.
- **A string de conexão de desenvolvimento está versionada** em
  `appsettings.json`. Serve para rodar localmente sem configurar nada; em
  produção o valor deve vir de `ESTOQUE_CONNECTIONSTRING` ou de um cofre de
  segredos, nunca do repositório.
- **Code-behind em vez de MVVM.** As páginas manipulam controles diretamente.
  Com `ViewModel` e *binding* bidirecional a lógica de tela ficaria testável
  junto com o resto.
- **A listagem carrega todos os produtos.** Sem paginação nem busca, o que
  basta para a escala do trabalho e não para um catálogo real.
- **`Frame` está obsoleto** desde o .NET 9 e ainda aparece nas telas de
  categoria e fornecedor; a de estoque já usa `Border`.
- **Os testes exigem um banco de verdade.** Não há *fake* do acesso a dados,
  então a suíte não roda sem MySQL disponível.

## Licença

[MIT](LICENSE)
