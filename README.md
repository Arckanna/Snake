# Snake

Jeu du Snake classique en C# et WPF. Guidez le serpent vers les fruits pour grandir et marquer des points — évitez les murs et votre propre corps.

## Fonctionnalités

- **Déplacement** : le serpent avance en continu ; changez de direction avec les touches.
- **Fruits** : mangez les fruits rouges pour gagner 1 point et allonger le serpent.
- **Murs** : si la tête touche un bord, la partie s’arrête (Game Over).
- **Corps** : si la tête touche une partie de son corps, la partie s’arrête (Game Over).
- **Score** : affiché dans le titre de la fenêtre.
- **Fond** : couleur crème (#F5F2EB) pour un bon contraste avec le serpent et les fruits.
- **Écran d’accueil** : titre « Snake » et bouton « Démarrer » pour lancer une partie.
- **Overlay Game Over** : en fin de partie, un écran « Perdu » avec le score et un bouton **Rejouer** pour relancer sans quitter la fenêtre.

## Contrôles

| Action | Touches |
|--------|---------|
| Haut | ↑ ou Z |
| Bas | ↓ ou S |
| Gauche | ← ou Q ou A |
| Droite | → ou D |

Les demi-tours (ex. droite → gauche) ne sont pas autorisés.

## Prérequis

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (ou supérieur)

## Installation et lancement

Cloner le dépôt, puis à la racine du projet :

```bash
# Restaurer les dépendances
dotnet restore

# Lancer le jeu
dotnet run
```

Ou compiler puis exécuter l’exécutable :

```bash
dotnet build
# Exécutable : bin/Debug/net8.0-windows/Snake.exe (ou bin/Release/...)
```

## Technologies

- **Langage** : C#
- **Framework** : WPF (Windows Presentation Foundation)
- **.NET** : 8.0
- **CommunityToolkit.Mvvm** : `ObservableObject`, `[RelayCommand]`, `INotifyPropertyChanged`
- **Microsoft.Extensions.DependencyInjection** : conteneur IoC au démarrage

## Architecture (MVVM)

Le projet est structuré en **MVVM** pour séparer la logique métier, la présentation et les décisions d’affichage.

| Couche | Rôle |
|--------|------|
| **Models** | `SnakePart`, `GameState`, `Direction` — données métier sans dépendance UI. |
| **Core** | `IGameEngine` / `GameEngine` — moteur de jeu pur (déplacement, collisions, nourriture), **testable sans WPF**. `GameConfig` centralise les constantes (dimensions, timer, taille des cases). |
| **Services** | `ITimerService` / `DispatcherTimerService` — abstraction du timer ; permet de mocker en tests. |
| **ViewModels** | `GameViewModel` (orchestre moteur + timer, expose Score, Title, `FrameUpdated`, commande Rejouer), `WelcomeViewModel` (commande Démarrer). |
| **Vues** | `MainWindow` (dessin du serpent/fruit, binding Title, overlay Game Over), `WelcomeWindow` (écran d’accueil). |

- **Injection de dépendances** : `IGameEngine`, `ITimerService` et `MainWindow` sont enregistrés dans `App.Application_Startup` ; `WelcomeWindow` reçoit `IServiceProvider` pour résoudre `MainWindow` au clic Démarrer.
- **Désabonnement** : à la fermeture de `MainWindow` et `WelcomeWindow`, les événements (`FrameUpdated`, `StartGameRequested`) et le timer sont correctement désabonnés/arrêtés pour éviter les fuites mémoire.

## Structure du projet

```
Snake/
├── App.xaml / App.xaml.cs         # Point d’entrée ; DI (IGameEngine, ITimerService, MainWindow) et affichage de WelcomeWindow
├── Core/
│   ├── GameConfig.cs              # Constantes : 700×400, 100 ms, 20 (carré), 10 (longueur init)
│   ├── IGameEngine.cs             # Interface du moteur de jeu
│   └── GameEngine.cs              # Implémentation (déplacement, collisions, nourriture)
├── Models/
│   ├── Direction.cs               # Left, Right, Up, Down
│   ├── GameState.cs               # NotStarted, Playing, GameOver
│   └── SnakePart.cs               # Segment (X, Y) sans référence UI
├── Services/
│   ├── ITimerService.cs           # Start(interval, onTick), Stop()
│   └── DispatcherTimerService.cs  # Implémentation WPF (DispatcherTimer)
├── ViewModels/
│   ├── GameViewModel.cs           # Score, Title, IsGameOver, Start, RejouerCommand, FrameUpdated
│   └── WelcomeViewModel.cs        # DemarrerCommand, StartGameRequested
├── MainWindow.xaml / .xaml.cs     # Zone de jeu 700×400, overlay Perdu + Rejouer, binding GameViewModel
├── WelcomeWindow.xaml / .xaml.cs  # Écran d’accueil, binding WelcomeViewModel
├── Snake.csproj
└── README.md
```

## Règles

1. La partie démarre avec un serpent de 10 carrés, une direction initiale vers la droite et un fruit sur la grille (paramètres dans `GameConfig`).
2. Un fruit mangé : +1 point, +1 segment, nouveau fruit à une case libre.
3. Tête dans un mur ou dans le corps du serpent : Game Over — overlay « Perdu », score affiché, bouton **Rejouer** pour recommencer.
4. Le jeu tourne en continu ; aucune limite de score.

---

*Projet à but pédagogique — démonstration de game dev en C#/WPF avec MVVM, IoC et séparation logique/UI.*
