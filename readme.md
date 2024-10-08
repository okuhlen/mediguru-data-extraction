# MediGuru Data Extraction Tool

The MediGuru data extraction tool is a C# console application which is used to extract raw medical procedures data and insert these into a database of your liking. This tool covers 2 parts of the process: namely the data extraction and the data structuring phase. This tool was used to structure medical procedures data used on the [MediGuru](https://www.mediguru.co.za) platform. The source code for the MediGuru Data Extraction Tool is released under the [Creative Commons 4.0 CC BY license](https://creativecommons.org/licenses/by/4.0/).

This readme provides a detailed explanation of the files contained in the project, and the configurations a developer might need to go through. Please read this document carefully before trying to run the console app as is. In short, you will be required to add some code of your own which handles storage of the extracted data.

Before getting started, please ensure you have .NET 8.0 installed. To download and install .NET 8.0, please visit the [Microsoft .NET site](https://dotnet.microsoft.com/en-us/download). You may use any IDE of your liking, that supports .NET projects (I used JetBrains Rider). 
## Step 1: Download the Required Files
Before getting started, there are a collection of files you will need to download and include with this app. The source files are not included in this version of the project, due to the lack of clarity surrounding the licensing under which the files are released. The healthcare providers do mention the files are for healthcare providers, but do not explicitly state any restrictions. As such, I can only provide links as to where the source files can be downloaded. To get started, please download the source files from these healthcare providers:

- Government Employee Medical Scheme (GEMS): [2024 Tariffs](https://www.gems.gov.za/Healthcare-Providers/Tariff-Files/2024-Tariff-Files?year=2024), and [2023 Tariffs](https://www.gems.gov.za/Healthcare-Providers/Tariff-Files/2023-Tariff-Files?year=2023).
- Momentum Health: [Tariff Files](https://provider.momentum.co.za/default.aspx?wv0/VQt%20352aqBYetE7mOzP25ni40mElMQCBtHeLFrhqZJoAkQmjjgI0R8l2eUTE3fR8oeIiUZc3QGgejEHXYA==)
- WoolTru Healthcare Fund: [WoolTru Healthcare Tariff Lookup Tool](https://www.wooltruhealthcarefund.co.za/benefits/fund-tariff--whft--lookup)
- CAMAF: [CAMAF Rates 2024](https://camaf.co.za/cbt-rates-2024/)
- List of Disciplines - create an excel document (xlsx) file, with all disciplines listed here: [BI Solutions](https://www.bisolutions.co.za/reports/disciplines.php)

If you would like a good idea as to how the downloaded files should be placed in the solution, please have a look at the following C# class files:

- `GEMSFileLocations.cs` - This file speaks to the expected locations of the GEMS excel spreadsheets. Place all the 2023 files in the GEMS folder. The 2024 files should be placed in the 2024 subdirectory in the GEMS folder.
- `BaseDirectories.cs` - This file speaks to the base directory where all Momentum files are to be found. Please ensure these files are in xlsx format!
  Other files which you should download/create:
- WoolTru files must be stored in the Files/WoolTru subdirectory.

After adding all the necessary files, do not forget to add these files to the project. Select all files and right click. Then, select properties. Set the "Copy to output directory" dropdown to "Copy if Newer". See the screenshot below as an example:

![Example of GEMS tariff file properties](Images/receipt_1.png)

If you find other healthcare providers who share some tariff rates data, feel free to modify the code, and open a pull request with the changes :-)

## Step 2: Store files in an appropriate structure in the console application

The next step of the process is to set up a database store to keep the extracted data. This solution makes use of [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) for MariaDB. The [Pomelo Entity Framework Core provider](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) is included to allow for this. If this is not to your liking, feel free to switch things up to whatever you like :)

To create the database model, you will need to create the schema in your DB system of choice (MariaDB was used in this project), and run a db migration to generate the tables. If you would like more information on doing this, please read the [MS documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli) (or ask ChatGPT if you dont have time for documentation).

If you would like a general sense of what tables will be created, please have a look at all the classes contained in the DatabaseModels folder. Here is a brief explanation of each table:

- Category: This keeps track of all possible categories
- Discipline: This keeps track of all doctors/discipline types
- MedicalAidName: keeps track of all medical aid names
- MedicalAidSchemeProcedure: This file keeps track of all the known medical aid procedures linked to a medical aid provider. This table is relevant only for user submissions (a user can supply medical records for any known medical aid scheme provider).
- Procedure: This table keeps track of all known procedures. During the initial run of the data extraction tool, all procedure information extracted from the spreadsheets and text files will be placed in this table.
- Provider: This table keeps track of all official providers (i.e.: those who have official data points that can be extracted from official data sources). In the case of this project, this table would list WoolTru, GEMS and Momentum.
- ProviderProcedure: This table keeps track of all procedures which have been linked to official providers (this is when the tool was able to extract pricing data points for a given procedure).

# Explanation of the class files used to extract data from excel/text files

The data extraction tool contains many file processors - most GEMS related file processors can be found in the GEMS folder. As you may notice, there are lots of GEMS file processors, and it can be argued that some kind of generic file processor could have been created. The reason there are so many GEMS file processors had to do with the the fact a lot of these files did not follow a consistent data structure. For example, some files had pricing points for many disciplines, while others have data points for 1 or 2 data points. For GEMS Non-Contracted files, the same "contracted" version of the C# class is used since both files (contracted vs the non contracted) had the same structures. All modifiers were excluded.

The `WoolTruFileProcessor.cs` is used to extract data from the WoolTru text files. The `MomentumFileProcessor.cs` is used to process all Momentum related files.

One more thing, you probably also want to have a look at the `ProcessFileParameters` class; this class is used to specify what file the respective processor should use, and how it should process the file.

# How the extracted data is used
NOTE: The data extraction task took a little over 1 hour to run - this could have been due to the fact I had a separate cloud server for development purposes. The execution time may differ, depending on your configuration.

Once the data extraction tool has been run successfully, all data points are stored in the Provider, ProviderProcedure, Procedure, Category and Discipline tables. Together, these tables maintain the following rules:

1. A provider can have 1 or more procedures.
2. A procedure can be linked to 1 or more disciplines/doctors.
3. A procedure can be associated to many categories.
4. Provider table should keep track of all medical aid providers, whose data can be extracted from official data sources.

At this point, you can derive some value from the raw extracted data. If you would like to see an example of how to extract value out of the data, continue reading on. The data extraction tool also comes with 2 Quartz.NET tasks which prepare the data for the elasticsearch index, and another task which indexes these data points for search purposes. These are explained below.

## Add Quartz.NET and Elasticsearch
If you would like to run the 2 tasks which handle data preparation and insertion into Elasticsearch, you will need to install and configure Quartz and elasticsearch.

1. To setup Quartz.NET, please have a look at the [documentation on the official website](https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/microsoft-di-integration.html) (or ask ChatGPT if you dont have time for documentation).
2. To set up elasticsearch, please visit the [documentation on the elasticsearch website](https://www.elastic.co/guide/en/elasticsearch/reference/current/install-elasticsearch.html#elasticsearch-install-packages) (or ask ChatGPT if you dont have time for documentation).

The NuGet packages for elasticsearch & quartz have been added to this data extraction tool.
## Look at the Quartz.NET Tasks which handle the Preparation of the Data and Insertion into Elasticsearch

The first step of this process is to take all the pre-populated data and get it in a state that allows for indexing into elasticsearch. Two database tables are populated with better structured data for elasticsearch:

- The SearchData table keeps track of all the medical procedures available for the medical aid schemes contained in this project.
- The SearchDataPoint table keeps track of all the individual data points which summarize an entry in the SearchData table. A medical procedure can have many data points, differentiated by price, category and/or discipline.

Have a look at the `SearchDataPreparer.cs` class (in the Tasks folder). This class is a Quartz.NET task which takes all official data points and structures them for elasticsearch. This task populates the SearchData and SearchDataPoint tables.

Next, take a look at the `SearchDataIndexerTask.cs` class (also in the Tasks folder). This task runs through all entries in the SearchData and SearchDataPoint tables and inserts them into the Elasticsearch index.

# Want to show your support?
If you would like to show your support, please consider [buying me a coffee](https://www.buymeacoffee.com/okuhlengado) :-)

# Some Frequently Asked Questions

1. Why are there so many GEMS related file processors, and why are some of these re-used? ANSWER: The reason to this is because many of the GEMS files have differing data structures. The initial idea was to create some generic file processor for GEMS, however, doing this made the code complicated to read (perhaps I shouldnt have skipped my 'Gang of Four' design pattern classes).  
2. Why release the source code for the data extraction tool? ANSWER: I release this code to make my contribution to researchers and other system developers who are studying or building tools which address some aspects of healthcare pricing transparency. From a South African context, there still exist many limitations with this app, which hopefully will be addressed in the near future. In the spirit of 'open science', it made sense for me to share my solution with those who are curious to know.

# How to contribute

If you would like to contribute to this tool, or have some bugs to report, or have questions, please open an issue, or pull request with your changes. Please leave the Karen/Kevin keyboard warrior and energy elsewhere! You may also send me a email or message and I will try my best to respond. Miss me with the Karen/Kevin energy. 