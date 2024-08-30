use FirmyCz;
go

CREATE TABLE Scrape (
	Id INT IDENTITY(1,1) PRIMARY KEY,
	ScrapeTime DATETIME DEFAULT GETDATE()
);

CREATE TABLE Category (
	Id INT PRIMARY KEY,
	[Name] VARCHAR(200) NOT NULL,
	[Path] VARCHAR(200) NOT NULL
);

INSERT INTO Category (Id, [Name], [Path]) VALUES
(1, 'Auto - moto', '/Auto-moto'),
(2, 'Cestovní služby', '/Cestovni-sluzby'),
(3, 'Restaurační a pohostinské služby', '/Restauracni-a-pohostinske-sluzby'),
(4, 'Elektro, mobily a počítače', '/Elektro-mobily-a-pocitace'),
(5, 'Banky a finanční služby', '/Banky-a-financni-sluzby'),
(6, 'Instituce a úřady', '/Instituce-a-urady'),
(7, 'Obchody a obchůdky', '/Obchody-a-obchudky'),
(8, 'Služby a řemesla', '/Remesla-a-sluzby'),
(9, 'Dům, byt a zahrada', '/Dum-byt-a-zahrada'),
(10, 'První pomoc a zdravotnictví', '/Prvni-pomoc-a-zdravotnictvi'),
(11, 'Vše pro firmy', '/Vse-pro-firmy'),
(12, 'Velkoobchod a výroba', '/Velkoobchod-a-vyroba')
;

CREATE TABLE ScrapedCompany (
	Id INT IDENTITY(1,1) PRIMARY KEY,
	ScrapeId INT NOT NULL,
	[Name] VARCHAR(255) NOT NULL,
	CategoryId INT NOT NULL,
	FirmyCzInternalId INT NOT NULL,
	[Path] VARCHAR(200) NOT NULL,
	[Description] VARCHAR(MAX),
	FOREIGN KEY (ScrapeId) REFERENCES Scrape(Id),
	FOREIGN KEY (CategoryId) REFERENCES Category(Id)
);

CREATE TABLE ScrapedCompanyInfo (
	Id INT IDENTITY(1,1) PRIMARY KEY,
	ScrapedCompanyId INT NOT NULL,
	[Label] VARCHAR(200) NOT NULL,
	[Value] VARCHAR(MAX),
	FOREIGN KEY (ScrapedCompanyId) REFERENCES ScrapedCompany(Id)
);

CREATE TABLE ScrapedCompanyTag (
	Id INT IDENTITY(1,1) PRIMARY KEY,
	ScrapedCompanyId INT NOT NULL,
	[Name] VARCHAR(80) NOT NULL,
	[Path] VARCHAR(200) NOT NULL,
	FOREIGN KEY (ScrapedCompanyId) REFERENCES ScrapedCompany(Id)
);

CREATE TABLE ScrapedCompanyCategory (
	Id INT IDENTITY(1,1) PRIMARY KEY,
	ScrapedCompanyId INT NOT NULL,
	[Name] VARCHAR(100) NOT NULL,
	[Path] VARCHAR(400) NOT NULL,
	FOREIGN KEY (ScrapedCompanyId) REFERENCES ScrapedCompany(Id)
);
