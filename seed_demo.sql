-- Execute com charset explícito, senão os acentos entram corrompidos:
--   mysql -h 127.0.0.1 -P 3307 -u root --default-character-set=utf8mb4 < seed_demo.sql

CREATE DATABASE IF NOT EXISTS controle_estoque_eletronicos
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE controle_estoque_eletronicos;

CREATE TABLE IF NOT EXISTS Categorias (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    nome         VARCHAR(100) NOT NULL
);
CREATE TABLE IF NOT EXISTS Fornecedores (
    id_fornecedor INT AUTO_INCREMENT PRIMARY KEY,
    nome_empresa  VARCHAR(150) NOT NULL,
    cnpj          VARCHAR(18)  NOT NULL
);
CREATE TABLE IF NOT EXISTS Produto (
    id_produto         INT AUTO_INCREMENT PRIMARY KEY,
    nome               VARCHAR(150)  NOT NULL,
    preco              DECIMAL(10,2) NOT NULL,
    quantidade_estoque INT           NOT NULL DEFAULT 0,
    id_categoria       INT           NOT NULL,
    id_fornecedor      INT           NOT NULL,
    CONSTRAINT fk_produto_categoria  FOREIGN KEY (id_categoria)  REFERENCES Categorias(id_categoria),
    CONSTRAINT fk_produto_fornecedor FOREIGN KEY (id_fornecedor) REFERENCES Fornecedores(id_fornecedor)
);
CREATE TABLE IF NOT EXISTS Pedidos_Vendas (
    id_pedido   INT AUTO_INCREMENT PRIMARY KEY,
    data_venda  DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    valor_total DECIMAL(10,2) NOT NULL
);
CREATE TABLE IF NOT EXISTS Itens_Pedido (
    id_item        INT AUTO_INCREMENT PRIMARY KEY,
    id_pedido      INT           NOT NULL,
    id_produto     INT           NOT NULL,
    quantidade     INT           NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_item_pedido  FOREIGN KEY (id_pedido)  REFERENCES Pedidos_Vendas(id_pedido),
    CONSTRAINT fk_item_produto FOREIGN KEY (id_produto) REFERENCES Produto(id_produto)
);

INSERT INTO Categorias (nome) VALUES
 ('Notebooks'),('Periféricos'),('Monitores'),('Armazenamento');

INSERT INTO Fornecedores (nome_empresa, cnpj) VALUES
 ('Distribuidora Vale Tech',   '12.345.678/0001-90'),
 ('Importadora Paulista',      '98.765.432/0001-10'),
 ('Nordeste Componentes',      '45.678.901/0001-55');

INSERT INTO Produto (nome, preco, quantidade_estoque, id_categoria, id_fornecedor) VALUES
 ('Notebook 14" Ryzen 5 16GB',   3499.00, 12, 1, 1),
 ('Notebook 15" Core i7 32GB',   6290.00,  4, 1, 2),
 ('Teclado mecânico ABNT2',       389.90, 27, 2, 1),
 ('Mouse sem fio 2.4GHz',         129.90, 45, 2, 3),
 ('Headset USB com microfone',    249.00,  8, 2, 3),
 ('Monitor 27" 144Hz IPS',       1899.00,  6, 3, 2),
 ('Monitor 24" Full HD',          949.00, 15, 3, 1),
 ('SSD NVMe 1TB',                 529.00, 33, 4, 2),
 ('HD externo 2TB',               449.00,  0, 4, 3),
 ('Pen drive 128GB',               79.90, 60, 4, 1);
