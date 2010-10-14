using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


public class AnimatedSprite
{
    public Texture2D Texture;       // Textur

    private float totalElapsed;   // Abgelaufene Zeit

    private int rows;             // Anzahl der Zeilen
    private int columns;          // Anzahl der Spalten
    private int width;            // Breite eines Einzelbilds
    private int height;           // H�he eines Einzelbilds
    private float animationSpeed; // Bilder pro Sekunde

    private int currentRow;       // Aktuelle Zeile
    private int currentColumn;    // Aktuelle Spalte

    public void LoadGraphic(
      Texture2D texture,
      int rows,
      int columns,
      int width,
      int height,
      int animationSpeed
      )
    {
        this.Texture = texture;
        this.rows = rows;
        this.columns = columns;
        this.width = width;
        this.height = height;
        this.animationSpeed = (float)1 / animationSpeed;

        totalElapsed = 0;
        currentRow = 0;
        currentColumn = 0;
    }

    public void Update(float elapsed)
    {
        totalElapsed += elapsed;
        if (totalElapsed > animationSpeed)
        {
            totalElapsed -= animationSpeed;

            currentColumn += 1;
            if (currentColumn >= columns)
            {
                currentRow += 1;
                currentColumn = 0;

                if (currentRow >= rows)
                {
                    currentRow = 0;
                }
            }

        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        spriteBatch.Draw(
            Texture,
            new Rectangle((int)position.X, (int)position.Y, width, height),
            new Rectangle(
              currentColumn * width,
              currentRow * height,
              width, height),
            color
            );
    }
}