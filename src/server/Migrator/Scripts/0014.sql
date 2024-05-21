ALTER TABLE $schema$.[JiraProfileProjects] DROP CONSTRAINT [FK_JiraProfileProjects_JiraProfiles_ClientId]

GO

ALTER TABLE $schema$.[JiraProfileAccounts] DROP CONSTRAINT [FK_JiraProfileAccounts_JiraProfiles_ClientId]

GO

DROP TABLE $schema$.[JiraProfiles] 

GO

ALTER TABLE $schema$.[JiraProfileProjects] ADD [TempoToken] nvarchar(200) NOT NULL

GO