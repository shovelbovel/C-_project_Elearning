-- init.sql: Schéma MySQL pour le projet Elearning
-- Ce script crée les tables utilisées par l'application et ajoute quelques données de base.

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS `Certificates`;
DROP TABLE IF EXISTS `Results`;
DROP TABLE IF EXISTS `Answers`;
DROP TABLE IF EXISTS `Questions`;
DROP TABLE IF EXISTS `Quizzes`;
DROP TABLE IF EXISTS `Lessons`;
DROP TABLE IF EXISTS `Courses`;
DROP TABLE IF EXISTS `Users`;
DROP TABLE IF EXISTS `Roles`;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE `Roles` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_Roles_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Users` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `FullName` VARCHAR(100) NOT NULL,
  `Email` VARCHAR(255) NOT NULL,
  `PasswordHash` VARCHAR(255) NOT NULL,
  `RoleId` INT NULL,
  `ThemePreference` VARCHAR(20) NOT NULL DEFAULT 'light',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_Users_Email` (`Email`),
  KEY `FK_Users_RoleId` (`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Courses` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Title` VARCHAR(200) NOT NULL,
  `Description` TEXT NULL,
  `Category` VARCHAR(100) NULL,
  `ImagePath` VARCHAR(255) NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Lessons` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Title` VARCHAR(200) NOT NULL,
  `Content` TEXT NULL,
  `CourseId` INT NOT NULL,
  `OrderIndex` INT NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `FK_Lessons_CourseId` (`CourseId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Quizzes` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Title` VARCHAR(200) NOT NULL,
  `CourseId` INT NOT NULL,
  `PassPercentage` INT NOT NULL DEFAULT 70,
  PRIMARY KEY (`Id`),
  KEY `FK_Quizzes_CourseId` (`CourseId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Questions` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Text` TEXT NOT NULL,
  `QuizId` INT NOT NULL,
  `Order` INT NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  KEY `FK_Questions_QuizId` (`QuizId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Answers` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `Text` TEXT NOT NULL,
  `IsCorrect` TINYINT(1) NOT NULL DEFAULT 0,
  `QuestionId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Answers_QuestionId` (`QuestionId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Results` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NOT NULL,
  `QuizId` INT NOT NULL,
  `Score` INT NOT NULL DEFAULT 0,
  `MaxScore` INT NOT NULL DEFAULT 0,
  `Passed` TINYINT(1) NOT NULL DEFAULT 0,
  `TakenAt` DATETIME NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Results_UserId` (`UserId`),
  KEY `FK_Results_QuizId` (`QuizId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `Certificates` (
  `Id` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NOT NULL,
  `CourseId` INT NULL,
  `ResultId` INT NULL,
  `IssuedAt` DATETIME NOT NULL,
  `Score` INT NOT NULL DEFAULT 0,
  `MaxScore` INT NOT NULL DEFAULT 0,
  `Passed` TINYINT(1) NOT NULL DEFAULT 0,
  `PdfPath` VARCHAR(255) NULL,
  `VerificationCode` VARCHAR(64) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Certificates_UserId` (`UserId`),
  KEY `FK_Certificates_CourseId` (`CourseId`),
  KEY `FK_Certificates_ResultId` (`ResultId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Foreign keys and cascade rules (alignés avec OnModelCreating)
ALTER TABLE `Users`
  ADD CONSTRAINT `FK_Users_Roles` FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE;

ALTER TABLE `Lessons`
  ADD CONSTRAINT `FK_Lessons_Courses` FOREIGN KEY (`CourseId`) REFERENCES `Courses`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `Quizzes`
  ADD CONSTRAINT `FK_Quizzes_Courses` FOREIGN KEY (`CourseId`) REFERENCES `Courses`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `Questions`
  ADD CONSTRAINT `FK_Questions_Quizzes` FOREIGN KEY (`QuizId`) REFERENCES `Quizzes`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `Answers`
  ADD CONSTRAINT `FK_Answers_Questions` FOREIGN KEY (`QuestionId`) REFERENCES `Questions`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `Results`
  ADD CONSTRAINT `FK_Results_Users` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Results_Quizzes` FOREIGN KEY (`QuizId`) REFERENCES `Quizzes`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `Certificates`
  ADD CONSTRAINT `FK_Certificates_Users` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Certificates_Courses` FOREIGN KEY (`CourseId`) REFERENCES `Courses`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT `FK_Certificates_Results` FOREIGN KEY (`ResultId`) REFERENCES `Results`(`Id`) ON DELETE SET NULL ON UPDATE CASCADE;

-- Données initiales (seed)
INSERT INTO `Roles` (`Name`) VALUES
  ('Admin'),
  ('Formateur'),
  ('Etudiant')
ON DUPLICATE KEY UPDATE `Name` = VALUES(`Name`);

INSERT INTO `Courses` (`Title`, `Description`, `Category`, `ImagePath`) VALUES
  ('Fondamentaux du développement web', 'Apprendre HTML, CSS et JavaScript pour créer des pages responsives et interactives.', 'Web', 'images/courses/web-development.svg'),
  ('Programmation en C# .NET 8', 'Découvrir la syntaxe C#, les concepts orientés objet et le développement d''applications avec .NET 8.', 'Programming', 'images/courses/programming-csharp.svg'),
  ('Introduction à la base de données MySQL', 'Concevoir des schémas, écrire des requêtes SQL, indexation et optimisation basiques.', 'Database', 'images/courses/database-mysql.svg')
ON DUPLICATE KEY UPDATE `Title` = VALUES(`Title`);

-- Dates par défaut pour les enregistrements qui en auront besoin
UPDATE `Results` SET `TakenAt` = NOW() WHERE `TakenAt` IS NULL;
UPDATE `Certificates` SET `IssuedAt` = NOW() WHERE `IssuedAt` IS NULL;

-- Fin du script
