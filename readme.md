
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

![Dotnet test](/Documentation/testexplorer.PNG)

Deploys podem ser feitos via Visual Studio (botão direito no projeto > Publish) ou via commits nos branchs plugados no CI/CD do Team Services. 

No branch develop, está plugado um processo de build que compila a solution e aplica os testes:

![Build](/Documentation/develop-build-definition.PNG)

Em casos de falha, esse processo abre work items e dispara e-mails para o time:

![Dotnet test](/Documentation/work-item-on-fail.PNG)

![Dotnet test](/Documentation/work-item-on-fail-2.PNG)

No branch master, pluguei um processo de build que gera o pacote para deploy imediato no Azure Web Apps:

![Dotnet test](/Documentation/production-build-definition.PNG)

