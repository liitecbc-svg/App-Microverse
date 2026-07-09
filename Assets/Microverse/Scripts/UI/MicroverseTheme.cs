/**
 * MicroverseTheme.cs
 *
 * Define la paleta visual compartida por los componentes de interfaz de Microverse.
 *
 * Main responsibilities:
 * - Centralizar colores de fondo, paneles y textos.
 * - Mantener consistencia entre pantallas y controles.
 * - Evitar valores de color duplicados en la UI.
 *
 * Related elements:
 * - UiFactory
 * - ModelCardView
 * - HomeScreenView
 */
using UnityEngine;

namespace Microverse.UI
{
    public static class MicroverseTheme
    {
        public static readonly Color Background = new Color(0.01f, 0.02f, 0.06f, 1f);
        public static readonly Color Panel = new Color(0.02f, 0.06f, 0.14f, 0.92f);
        public static readonly Color PanelLight = new Color(0.05f, 0.11f, 0.24f, 0.92f);
        public static readonly Color Stroke = new Color(0.16f, 0.27f, 0.52f, 0.8f);
        public static readonly Color Cyan = new Color(0.0f, 0.88f, 1.0f, 1f);
        public static readonly Color Blue = new Color(0.0f, 0.35f, 1.0f, 1f);
        public static readonly Color Purple = new Color(0.7f, 0.28f, 1.0f, 1f);
        public static readonly Color Text = new Color(0.95f, 0.97f, 1f, 1f);
        public static readonly Color MutedText = new Color(0.68f, 0.74f, 0.88f, 1f);
        public static readonly Color DimText = new Color(0.45f, 0.53f, 0.72f, 1f);
    }
}
