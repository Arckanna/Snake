# Snake

Jeu du Snake classique en C# et WPF. Guidez le serpent vers les fruits pour grandir et marquer des points — évitez les murs.

## Fonctionnalités

- **Déplacement** : le serpent avance en continu ; changez de direction avec les touches.
- **Fruits** : mangez les fruits rouges pour gagner 1 point et allonger le serpent.
- **Murs** : si la tête touche un bord, la partie s’arrête (Game Over).
- **Corps** : si la tête touche une partie de son corps, la partie s’arrête (Game Over).
- **Score** : affiché dans le titre de la fenêtre.
- **Fond** : couleur crème (#F5F2EB) pour un bon contraste avec le serpent et les fruits.

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

## Structure du projet

```
Snake/
├── App.xaml / App.xaml.cs    # Point d’entrée WPF
├── MainWindow.xaml           # Fenêtre et zone de jeu (Canvas 700×400)
├── MainWindow.xaml.cs        # Logique : serpent, fruits, murs, score, timer
├── SnakePart.cs              # Modèle d’un segment du serpent (Position, UiElement)
├── Snake.csproj
└── README.md
```

## Règles

1. La partie démarre avec un serpent de 10 carrés, une direction initiale vers la droite et un fruit sur la grille.
2. Un fruit mangé : +1 point, +1 segment, nouveau fruit à une case libre.
3. Tête dans un mur ou dans le corps du serpent : Game Over (MessageBox + titre mis à jour).
4. Le jeu tourne en continu ; aucune limite de score.

---

*Projet à but pédagogique — démonstration de game dev en C#/WPF.*
