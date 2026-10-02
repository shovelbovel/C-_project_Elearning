# Plateforme E-Learning (ASP.NET MVC)

Application web de gestion de formation en ligne, développée en C# avec ASP.NET MVC.
Elle couvre la consultation de cours, le suivi de progression des apprenants et la
génération de certificats.

> **À COMPLÉTER (1)** : remplacez le paragraphe ci-dessus par une description en deux
> phrases de ce que fait réellement l'application. Gardez-la factuelle.

---

## Stack technique

| Couche | Technologies |
|---|---|
| Backend | C#, ASP.NET MVC (.NET **À COMPLÉTER (2)** : indiquez la version, ex. .NET 8) |
| Architecture | Modèle MVC (Controllers / Models / Views) |
| Accès aux données | Couche `Data/`, base de données relationnelle |
| Frontend | HTML, CSS, JavaScript |
| Configuration | `appsettings.json` |

---

## Fonctionnalités

- Gestion des cours et des contenus pédagogiques
- Suivi de la progression des apprenants
- Génération de certificats de fin de formation
- **À COMPLÉTER (3)** : listez ici 3 à 5 fonctionnalités réellement implémentées.
  Supprimez celles de la liste ci-dessus qui n'existent pas dans le code.

---

## Architecture du projet

```
Controllers/     Contrôleurs MVC : routage des requêtes et logique applicative
Models/          Modèles de données et entités métier
Views/           Vues Razor rendues côté serveur
Data/            Couche d'accès aux données et contexte de persistance
Database/        Scripts et fichiers de base de données
certificates/    Ressources liées à la génération des certificats
css/             Feuilles de style
js/              Scripts côté client
icons/           Ressources graphiques
Program.cs       Point d'entrée de l'application
appsettings.json Configuration de l'application
```

---

## Installation et lancement

**Prérequis** : SDK .NET **À COMPLÉTER (2)** et une instance de base de données.

```bash
# Cloner le dépôt
git clone https://github.com/shovelbovel/C-_project_Elearning.git
cd C-_project_Elearning

# Restaurer les dépendances
dotnet restore

# Configurer la chaîne de connexion dans appsettings.json
# À COMPLÉTER (4) : indiquez le nom exact du paramètre, ex. "DefaultConnection"

# Lancer l'application
dotnet run
```

L'application est ensuite accessible sur `https://localhost:5001`
(**À COMPLÉTER (5)** : vérifiez le port réel dans `launchSettings.json`).

---

## Contexte

Projet académique réalisé dans le cadre du cycle ingénieur en Intelligence
Artificielle et Data Science à l'EMSI, École Marocaine des Sciences de l'Ingénieur,
Casablanca.

Objectif pédagogique : mettre en pratique l'architecture MVC, la séparation des
responsabilités et le développement web côté serveur avec l'écosystème .NET.

---

## Auteur

**Chouaib Iktache**
Élève-ingénieur IA & Data Science, EMSI Casablanca
[LinkedIn](https://linkedin.com/in/chouaib-iktache-076615340) · [GitHub](https://github.com/shovelbovel)
