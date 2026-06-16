# Guide du Projet PboWasm

Ce document explique l'architecture technique et les choix technologiques de ce projet Blazor WebAssembly.

## Architecture CSS : Tailwind CSS + DaisyUI

Bien que des dossiers "Bootstrap" puissent être présents (résidus du template par défaut de Microsoft), ce projet utilise exclusivement **Tailwind CSS** et **DaisyUI** pour son interface.

### Pourquoi voyez-vous du Bootstrap ?
Le dossier `wwwroot/lib/bootstrap` est créé par défaut lors de la génération d'un projet Blazor. Dans ce projet, il n'est **pas référencé** dans `wwwroot/index.html` et n'est donc pas chargé par le navigateur. Vous pouvez l'ignorer ou le supprimer.

### Tailwind CSS
Tailwind est un framework CSS "utility-first" qui permet de styliser les composants directement dans le HTML/Razor via des classes comme `flex`, `p-4`, `text-center`.

### DaisyUI
DaisyUI est une extension de Tailwind qui fournit des composants pré-stylisés (boutons, navbars, cards, modals) utilisant des classes sémantiques.
*   **Exemples utilisés** : `navbar`, `btn-primary`, `footer`, `loading-spinner`.
*   **Thèmes** : Le projet supporte les thèmes DaisyUI (configurés dans `tailwind.config.js`).

---

## Structure des Fichiers Clés

### Configuration
*   `package.json` : Contient les dépendances Node.js (Tailwind, DaisyUI, PostCSS) et les scripts de compilation.
*   `tailwind.config.js` : Configuration de Tailwind. C'est ici que l'on définit les fichiers à scanner (`.razor`) et les plugins comme DaisyUI.
*   `postcss.config.js` : Configuration du processeur CSS.

### Styles
*   `Styles/app.css` : **Source CSS**. C'est ici que l'on écrit du CSS personnalisé si nécessaire et où les directives `@tailwind` sont déclarées.
*   `wwwroot/css/app.css` : **CSS Compilé**. Ce fichier est généré automatiquement. **Ne le modifiez jamais directement**, car vos changements seront écrasés.

---

## Workflow de Développement (CSS)

Pour que vos modifications de classes Tailwind soient prises en compte, le CSS doit être recompilé.

### Commandes NPM
Ouvrez un terminal à la racine du projet :

1.  **Compilation ponctuelle** :
    ```bash
    npm run build:css
    ```
2.  **Mode "Watch" (recommandé)** : Compile automatiquement à chaque modification de fichier `.razor`.
    ```bash
    npm run watch:css
    ```

---

## Composants Principaux

*   `Layout/MainLayout.razor` : Définit la structure globale (Navbar, Footer) avec DaisyUI.
*   `wwwroot/index.html` : Point d'entrée HTML qui charge `css/app.css`.
*   `Services/QrScannerService.cs` : Service C# gérant l'interaction avec le scanner QR (JS Interop).
*   `wwwroot/js/qrScanner.js` : Logique JavaScript pour l'accès caméra et le scan via `jsQR`.
