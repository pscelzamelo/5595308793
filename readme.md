
## Sobre o projeto

Este projeto trata-se de um exercício para atendimento ao desafio [Zx Ventures - Backend](https://github.com/ZXVentures/code-challenge/blob/master/backend.md).
Na primeira versão enviada, entendi que tratava-se de um exercício de programação para validar conhecimentos técnicos. Havendo entendido que a expectativa na verdade era não somente de um exercício de programação mas na visualização de habilidades de engenharia de software, fiz uma segunda versão mais elaborada.

## Sobre a Implementação

Migrei o projeto da versão mais recente do .Net Framework para o .Net Core, pelos seguintes motivos:

- O time a avaliar o projeto usa linux, logo sugerir para a execução Windows + Visual Studio foi bola fora!
- Apesar do .Net Core ser muito recente, saiu em Agosto/2017 sua segunda versão estável, a .Net Core 2.0.0. Na vida real sou cuidadoso com utilizar "novidades" saindo do forno, mas como aqui trata-se de um teste e a plataforma está bem mais estável e atrativa agora, pareceu uma excelente idéia.

Para ilustrar um cenário de execução real, realizei o deploy da solution no [Azure Web Apps](https://azure.microsoft.com/en-us/services/app-service/web/), e configurei CI/CD no projeto via [Visual Studio Team Services](https://www.visualstudio.com/team-services/):

- Visual Studio Team Services: https://pscelzamelo.visualstudio.com/zx/_home
- Azure Web App em produção: http://zxbackend20170826095451.azurewebsites.net/ 

Caso desejem visualizar a parametrização do CI/CD no Visual Studio Team Services (recomendo!!), enviem um endereço de e-mail que crio um usuário, ou mostro ao vivo!

## Instruções para execução

O projeto depende do .Net Core. A instalação é muito simples inclusive em OSX/Linux:

https://www.microsoft.com/net/core

Basta instalar, e executar o projeto com **dotnet run** na pasta do projeto web:

![Dotnet run](/Documentation/dotnetrun.png)

Para desenvolvimento é razoável utilizar tanto o Visual Studio Community 2017 (Windows) quanto o Visual Studio Code (Windows, OSX, Linux).

Para executar os testes, **dotnet test ZxBackend.Tests/ZxBackend.Tests.csproj**:

![Dotnet test](/Documentation/testexecution.PNG)

Outra forma de executar os testes é via Visual Studio na aba Test Explorer:

![Dotnet test](/Documentation/testexplorer.png)

Deploys podem ser feitos via Visual Studio (botão direito no projeto > Publish) ou via commits nos branchs plugados no CI/CD do Team Services. 

No branch develop, está plugado um processo de build que compila a solution e aplica os testes:

![Build](/Documentation/develop-build-definition.PNG)

Em casos de falha, esse processo abre work items e dispara e-mails para o time:

![Dotnet test](/Documentation/work-item-on-fail.PNG)

![Dotnet test](/Documentation/work-item-on-fail-2.PNG)

No branch master, pluguei um processo de build que gera o pacote para deploy imediato no Azure Web Apps:

![Dotnet test](/Documentation/production-build-definition.PNG)

## Sobre o código

Parti do template de Web Api do .Net Core, criando o **ZxVentures\Controllers\PdvController.cs** para abrigar a API, e o **ZxVentures\Utils\GeoUtils.cs** para abrigar a lógica referente à geolocalização. No PdvController são injetadas dependências para acesso a cache e banco de dados via construtor e o framework de DI nativo do .Net Core.

Para acesso a dados, fiz uso do Entity Framework, um ORM nativo do .Net que uso extensivamente há anos. A classe **ZxVentures\Data\AppDbContext.cs** define o contexto, a **ZxVentures\Data\DbInitializer.cs** contém a lógica de inicialização da banco - carga a partir do json fornecido, e os controllers chamam as propriedades do contexto diretamente para acesso fisico às tabelas. 

Para os testes, como as dependências são injetadas via construtor, mockar o contexto é muito simples. Essa versão mais recente do EntityFramework possui ainda uma maneira trivial de orientá-lo a fazer uso de persistência em memória, o que permite subir um contexto temporário executando minha lógica de inicialização do banco inclusive:

![Dotnet test](/Documentation/mockup-banco.PNG)

As strings de conexão de banco estão em **ZxVentures\appsettings.json**, **ZxVentures\appsettings.Development.json** e **ZxVentures\appsettings.Production.json**. Conforme o ambiente em que a aplicação roda, o .Net Core aplica as transformações dos arquivos filho no arquivo pai, o que permite que eu especifique uma connection string de um banco diferente para o ambiente de produção, por exemplo:

![Dotnet test](/Documentation/connection-strings.PNG)

Nesse caso, no ambiente de desenvolvimento o banco de dados utilizado é o SQL Server LocalDB, versão minimalista do SQL Server que vem shipada com o Visual Studio. Em produção, utilizei um serviço free para testes. Como o DbInitializer cria as tabelas à partir das classes enumeradas no contexto, a implementação foi trivial.

Gosto de basear minhas API's em CQRS (Command and Query Responsibility Segregation), separando as operações que tratam-se de consultas (querys - lista pdv, retorna pdv por id, retorna pdv mais próximo) das operações de comando - operações efetivas nos dados sob gestão (nesse caso, somente criar um novo PDV, que altera meu banco). 

Entendo que o ideal seria segregar os métodos implementando controllers diferentes com dependências diferentes, onde os QueryControllers recebem serviços de cache e acesso a dados somente leitura, e os CommandControllers recebem serviços com privilégios e possuem uma mensageria padrão para retorno,  tornando o uso da API por clientes mais amigável. A classe **ZxVentures\Models\CommandResponse.cs** implementa o padrão de mensagens. A criação de PDV (unico "command" do desafio), retorna sempre uma resposta HTTP 200 mesmo em casos de falha:

![Dotnet test](/Documentation/command-response.PNG)

Apesar de sugerir segregar, mantive tudo junto nesse projeto para não causar estranheza ao padrão RESTful. No mesmo endpoint (url) estão todos os serviços, e a intenção do cliente dentre criar um Pdv ou Listar um Pdv é mencionado via os verbos HTTP - Post e Get.

Finalizando, acho interessante enumerar que dentre os componentes utilizados, todos são nativos do .Net. A única exceção é o componente GeoJson.Net - pacote externo que traz as classes a deserializar do formato GeoJson. No design das minhas soluções, levo em conta sempre as skills do time-alvo a manter o projeto, e tendo acessível via o portal de documentação da Microsoft praticamente tudo de conhecimento necessário para entender a solution, consigo trabalhar com uma gama de senioridades maior e tenho uma curva de aprendizado reduzida. 

## Bonus: Docker

Por tratar a maior parte do meu tempo de aplicações .Net em ambiente windows, o caminho mais óbvio foi naturalmente apresentar uma solução .Net com recursos do Azure. Ciente de que na Zx a stack é baseada em Python e sem Windows, achei que seria válido aproveitar a possibilidade do .Net Core de rodar em Linux para realizar testes rodando a aplicação desenvolvida em containers Docker. Para minha surpresa, o Visual Studio Community 2017 possui um recurso para adicionar à uma solution existente os arquivos necessários para subir os containers e executa a aplicação finalmente em Docker :

![Dotnet test](/Documentation/docker-files-generation.PNG)

Após a criação dos arquivos, é acrescentado à solution um Projeto Docker, onde na sua build ele executa o docker-compose e prepara os containers:

![Dotnet test](/Documentation/docker-build.PNG)

Clicando em run, os containers sobem:

![Dotnet test](/Documentation/app-rodando-docker.PNG)

*Disclaimer: Não tenho muita experiência com docker. Já fiz uso no passado para provas de conceito mas novamente, por estar "preso" em soluções Windows e IIS, nunca me aprofundei nos estudos, muito embora gostaria muito!!!*

## Troubleshooting

O ambiente onde executei, testei e tirei os prints trata-se de Windows 10, com o Visual Studio 2017 Community e o .Net Core 2.0.0 instalados apenas. 

Desconfio que eventualmente um "dotnet run" numa máquina Linux não vá rodar de primeira por padrão, já que o ambiente de desenvolvimento faz uso do SQL Server LocalDb. Em Linux entendo que rodaria na configuração de Production, muito embora não tive oportunidade de testar. Igualmente para os containers docker.