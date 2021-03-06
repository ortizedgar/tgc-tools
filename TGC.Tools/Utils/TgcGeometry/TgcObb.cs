﻿using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Tools.Model;
using TGC.Tools.Utils.Shaders;
using TGC.Tools.Utils.TgcSceneLoader;

namespace TGC.Tools.Utils.TgcGeometry
{
    /// <summary>
    ///     Representa un Orientend-BoundingBox (OBB)
    /// </summary>
    public class TgcObb : IRenderObject
    {
        private bool dirtyValues;

        protected Effect effect;

        private Vector3 extents;

        private Vector3[] orientation = new Vector3[3];

        protected string technique;

        private CustomVertex.PositionColored[] vertices;

        /// <summary>
        ///     Construir OBB vacio
        /// </summary>
        public TgcObb()
        {
            RenderColor = Color.Yellow.ToArgb();
            dirtyValues = true;
            AlphaBlendEnable = false;
        }

        /// <summary>
        ///     Centro
        /// </summary>
        public Vector3 Center
        {
            get { return Position; }
            set
            {
                Position = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Orientacion del OBB, expresada en local axes
        /// </summary>
        public Vector3[] Orientation
        {
            get { return orientation; }
            set
            {
                orientation = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Radios
        /// </summary>
        public Vector3 Extents
        {
            get { return extents; }
            set
            {
                extents = value;
                dirtyValues = true;
            }
        }

        /// <summary>
        ///     Color de renderizado del BoundingBox.
        /// </summary>
        public int RenderColor { get; private set; }

        public Vector3 Position { get; private set; }

        /// <summary>
        ///     Shader del mesh
        /// </summary>
        public Effect Effect
        {
            get { return effect; }
            set { effect = value; }
        }

        /// <summary>
        ///     Technique que se va a utilizar en el effect.
        ///     Cada vez que se llama a render() se carga este Technique (pisando lo que el shader ya tenia seteado)
        /// </summary>
        public string Technique
        {
            get { return technique; }
            set { technique = value; }
        }

        /// <summary>
        ///     Habilita el renderizado con AlphaBlending para los modelos
        ///     con textura o colores por vértice de canal Alpha.
        ///     Por default está deshabilitado.
        /// </summary>
        public bool AlphaBlendEnable { get; set; }

        /// <summary>
        ///     Renderizar
        /// </summary>
        public void render()
        {
            var d3dDevice = GuiController.Instance.D3dDevice;
            var texturesManager = GuiController.Instance.TexturesManager;

            texturesManager.clear(0);
            texturesManager.clear(1);

            //Cargar shader si es la primera vez
            if (effect == null)
            {
                effect = GuiController.Instance.Shaders.VariosShader;
                technique = TgcShaders.T_POSITION_COLORED;
            }

            //Actualizar vertices de BoundingBox solo si hubo una modificación
            if (dirtyValues)
            {
                updateValues();
                dirtyValues = false;
            }

            GuiController.Instance.Shaders.setShaderMatrixIdentity(effect);
            d3dDevice.VertexDeclaration = GuiController.Instance.Shaders.VdecPositionColored;
            effect.Technique = technique;

            //Render con shader
            effect.Begin(0);
            effect.BeginPass(0);
            d3dDevice.DrawUserPrimitives(PrimitiveType.LineList, 12, vertices);
            effect.EndPass();
            effect.End();
        }

        /// <summary>
        ///     Libera los recursos del objeto
        /// </summary>
        public void dispose()
        {
            vertices = null;
        }

        /// <summary>
        ///     Configurar el color de renderizado del OBB
        ///     Ejemplo: Color.Yellow.ToArgb();
        /// </summary>
        public void setRenderColor(Color color)
        {
            RenderColor = color.ToArgb();
            dirtyValues = true;
        }

        /// <summary>
        ///     Actualizar los valores de los vertices a renderizar
        /// </summary>
        public void updateValues()
        {
            if (vertices == null)
            {
                vertices = vertices = new CustomVertex.PositionColored[24];
            }

            var corners = computeCorners();

            //Cuadrado de atras
            vertices[0] = new CustomVertex.PositionColored(corners[0], RenderColor);
            vertices[1] = new CustomVertex.PositionColored(corners[4], RenderColor);

            vertices[2] = new CustomVertex.PositionColored(corners[0], RenderColor);
            vertices[3] = new CustomVertex.PositionColored(corners[2], RenderColor);

            vertices[4] = new CustomVertex.PositionColored(corners[2], RenderColor);
            vertices[5] = new CustomVertex.PositionColored(corners[6], RenderColor);

            vertices[6] = new CustomVertex.PositionColored(corners[4], RenderColor);
            vertices[7] = new CustomVertex.PositionColored(corners[6], RenderColor);

            //Cuadrado de adelante
            vertices[8] = new CustomVertex.PositionColored(corners[1], RenderColor);
            vertices[9] = new CustomVertex.PositionColored(corners[5], RenderColor);

            vertices[10] = new CustomVertex.PositionColored(corners[1], RenderColor);
            vertices[11] = new CustomVertex.PositionColored(corners[3], RenderColor);

            vertices[12] = new CustomVertex.PositionColored(corners[3], RenderColor);
            vertices[13] = new CustomVertex.PositionColored(corners[7], RenderColor);

            vertices[14] = new CustomVertex.PositionColored(corners[5], RenderColor);
            vertices[15] = new CustomVertex.PositionColored(corners[7], RenderColor);

            //Union de ambos cuadrados
            vertices[16] = new CustomVertex.PositionColored(corners[0], RenderColor);
            vertices[17] = new CustomVertex.PositionColored(corners[1], RenderColor);

            vertices[18] = new CustomVertex.PositionColored(corners[4], RenderColor);
            vertices[19] = new CustomVertex.PositionColored(corners[5], RenderColor);

            vertices[20] = new CustomVertex.PositionColored(corners[2], RenderColor);
            vertices[21] = new CustomVertex.PositionColored(corners[3], RenderColor);

            vertices[22] = new CustomVertex.PositionColored(corners[6], RenderColor);
            vertices[23] = new CustomVertex.PositionColored(corners[7], RenderColor);
        }

        /// <summary>
        ///     Crea un array con los 8 vertices del OBB
        /// </summary>
        private Vector3[] computeCorners()
        {
            var corners = new Vector3[8];

            var eX = extents.X * orientation[0];
            var eY = extents.Y * orientation[1];
            var eZ = extents.Z * orientation[2];

            corners[0] = Position - eX - eY - eZ;
            corners[1] = Position - eX - eY + eZ;

            corners[2] = Position - eX + eY - eZ;
            corners[3] = Position - eX + eY + eZ;

            corners[4] = Position + eX - eY - eZ;
            corners[5] = Position + eX - eY + eZ;

            corners[6] = Position + eX + eY - eZ;
            corners[7] = Position + eX + eY + eZ;

            return corners;
        }

        /// <summary>
        ///     Mueve el centro del OBB
        /// </summary>
        /// <param name="movement">Movimiento relativo que se quiere aplicar</param>
        public void move(Vector3 movement)
        {
            Position += movement;
            dirtyValues = true;
        }

        /// <summary>
        ///     Rotar OBB en los 3 ejes.
        ///     Es una rotacion relativa, sumando a lo que ya tenia antes de rotacion.
        /// </summary>
        /// <param name="movement">Ángulo de rotación de cada eje en radianes</param>
        public void rotate(Vector3 rotation)
        {
            var rotM = Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            var currentRotM = computeRotationMatrix();
            var newRotM = currentRotM * rotM;

            orientation[0] = new Vector3(newRotM.M11, newRotM.M12, newRotM.M13);
            orientation[1] = new Vector3(newRotM.M21, newRotM.M22, newRotM.M23);
            orientation[2] = new Vector3(newRotM.M31, newRotM.M32, newRotM.M33);

            dirtyValues = true;
        }

        /// <summary>
        ///     Cargar la rotacion absoluta del OBB.
        ///     Pierda la rotacion anterior.
        /// </summary>
        /// <param name="rotation">Ángulo de rotación de cada eje en radianes</param>
        public void setRotation(Vector3 rotation)
        {
            var rotM = Matrix.RotationYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            orientation[0] = new Vector3(rotM.M11, rotM.M12, rotM.M13);
            orientation[1] = new Vector3(rotM.M21, rotM.M22, rotM.M23);
            orientation[2] = new Vector3(rotM.M31, rotM.M32, rotM.M33);

            dirtyValues = true;
        }

        /// <summary>
        ///     Calcula la matriz de rotacion 4x4 del Obb en base a su orientacion
        /// </summary>
        /// <returns>Matriz de rotacion de 4x4</returns>
        public Matrix computeRotationMatrix()
        {
            var rot = Matrix.Identity;

            rot.M11 = orientation[0].X;
            rot.M12 = orientation[0].Y;
            rot.M13 = orientation[0].Z;

            rot.M21 = orientation[1].X;
            rot.M22 = orientation[1].Y;
            rot.M23 = orientation[1].Z;

            rot.M31 = orientation[2].X;
            rot.M32 = orientation[2].Y;
            rot.M33 = orientation[2].Z;

            return rot;
        }

        /// <summary>
        ///     Calcular OBB a partir de un conjunto de puntos.
        ///     Busca por fuerza bruta el mejor OBB en la mejor orientación que se ajusta a esos puntos.
        ///     Es un calculo costoso.
        /// </summary>
        /// <param name="points">puntos</param>
        /// <returns>OBB calculado</returns>
        public static TgcObb computeFromPoints(Vector3[] points)
        {
            return computeFromPointsRecursive(points, new Vector3(0, 0, 0), new Vector3(360, 360, 360), 10f).toClass();
        }

        /// <summary>
        ///     Calcular OBB a partir de un conjunto de puntos.
        ///     Prueba todas las orientaciones entre initValues y endValues, saltando de angulo en cada intervalo segun step
        ///     Continua recursivamente hasta llegar a un step menor a 0.01f
        /// </summary>
        /// <returns></returns>
        private static OBBStruct computeFromPointsRecursive(Vector3[] points, Vector3 initValues, Vector3 endValues,
            float step)
        {
            var minObb = new OBBStruct();
            var minVolume = float.MaxValue;
            var minInitValues = Vector3.Empty;
            var minEndValues = Vector3.Empty;
            var transformedPoints = new Vector3[points.Length];
            float x, y, z;

            x = initValues.X;
            while (x <= endValues.X)
            {
                y = initValues.Y;
                var rotX = FastMath.ToRad(x);
                while (y <= endValues.Y)
                {
                    z = initValues.Z;
                    var rotY = FastMath.ToRad(y);
                    while (z <= endValues.Z)
                    {
                        //Matriz de rotacion
                        var rotZ = FastMath.ToRad(z);
                        var rotM = Matrix.RotationYawPitchRoll(rotY, rotX, rotZ);
                        Vector3[] orientation =
                        {
                            new Vector3(rotM.M11, rotM.M12, rotM.M13),
                            new Vector3(rotM.M21, rotM.M22, rotM.M23),
                            new Vector3(rotM.M31, rotM.M32, rotM.M33)
                        };

                        //Transformar todos los puntos a OBB-space
                        for (var i = 0; i < transformedPoints.Length; i++)
                        {
                            transformedPoints[i].X = Vector3.Dot(points[i], orientation[0]);
                            transformedPoints[i].Y = Vector3.Dot(points[i], orientation[1]);
                            transformedPoints[i].Z = Vector3.Dot(points[i], orientation[2]);
                        }

                        //Obtener el AABB de todos los puntos transformados
                        var aabb = TgcBoundingBox.computeFromPoints(transformedPoints);

                        //Calcular volumen del AABB
                        var extents = aabb.calculateAxisRadius();
                        extents = TgcVectorUtils.abs(extents);
                        var volume = extents.X * 2 * extents.Y * 2 * extents.Z * 2;

                        //Buscar menor volumen
                        if (volume < minVolume)
                        {
                            minVolume = volume;
                            minInitValues = new Vector3(x, y, z);
                            minEndValues = new Vector3(x + step, y + step, z + step);

                            //Volver centro del AABB a World-space
                            var center = aabb.calculateBoxCenter();
                            center = center.X * orientation[0] + center.Y * orientation[1] + center.Z * orientation[2];

                            //Crear OBB
                            minObb.center = center;
                            minObb.extents = extents;
                            minObb.orientation = orientation;
                        }

                        z += step;
                    }
                    y += step;
                }
                x += step;
            }

            //Recursividad en mejor intervalo encontrado
            if (step > 0.01f)
            {
                minObb = computeFromPointsRecursive(points, minInitValues, minEndValues, step / 10f);
            }

            return minObb;
        }

        /// <summary>
        ///     Generar OBB a partir de AABB
        /// </summary>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>OBB generado</returns>
        public static TgcObb computeFromAABB(TgcBoundingBox aabb)
        {
            return computeFromAABB(aabb.toStruct()).toClass();
        }

        /// <summary>
        ///     Generar OBB a partir de AABB
        /// </summary>
        /// <param name="aabb">BoundingBox</param>
        /// <returns>OBB generado</returns>
        public static OBBStruct computeFromAABB(TgcBoundingBox.AABBStruct aabb)
        {
            var obb = new OBBStruct();
            obb.extents = (aabb.max - aabb.min) * 0.5f;
            obb.center = aabb.min + obb.extents;

            obb.orientation = new[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };
            return obb;
        }

        /// <summary>
        ///     Convertir un punto de World-Space espacio de coordenadas del OBB (OBB-Space)
        /// </summary>
        /// <param name="p">Punto en World-space</param>
        /// <returns>Punto convertido a OBB-space</returns>
        public Vector3 toObbSpace(Vector3 p)
        {
            var t = p - Position;
            return new Vector3(Vector3.Dot(t, orientation[0]), Vector3.Dot(t, orientation[1]),
                Vector3.Dot(t, orientation[2]));
        }

        /// <summary>
        ///     Convertir un punto de OBB-space a World-space
        /// </summary>
        /// <param name="p">Punto en OBB-space</param>
        /// <returns>Punto convertido a World-space</returns>
        public Vector3 toWorldSpace(Vector3 p)
        {
            return Position + p.X * orientation[0] + p.Y * orientation[1] + p.Z * orientation[2];
        }

        /// <summary>
        ///     Convertir a struct
        /// </summary>
        public OBBStruct toStruct()
        {
            var obbStruct = new OBBStruct();
            obbStruct.center = Position;
            obbStruct.orientation = orientation;
            obbStruct.extents = extents;
            return obbStruct;
        }

        /// <summary>
        ///     OBB en un struct liviano
        /// </summary>
        public struct OBBStruct
        {
            public Vector3 center;
            public Vector3[] orientation;
            public Vector3 extents;

            /// <summary>
            ///     Convertir a clase
            /// </summary>
            public TgcObb toClass()
            {
                var obb = new TgcObb();
                obb.Position = center;
                obb.orientation = orientation;
                obb.extents = extents;
                return obb;
            }

            /// <summary>
            ///     Convertir un punto de World-Space espacio de coordenadas del OBB (OBB-Space)
            /// </summary>
            /// <param name="p">Punto en World-space</param>
            /// <returns>Punto convertido a OBB-space</returns>
            public Vector3 toObbSpace(Vector3 p)
            {
                var t = p - center;
                return new Vector3(Vector3.Dot(t, orientation[0]), Vector3.Dot(t, orientation[1]),
                    Vector3.Dot(t, orientation[2]));
            }

            /// <summary>
            ///     Convertir un punto de OBB-space a World-space
            /// </summary>
            /// <param name="p">Punto en OBB-space</param>
            /// <returns>Punto convertido a World-space</returns>
            public Vector3 toWorldSpace(Vector3 p)
            {
                return center + p.X * orientation[0] + p.Y * orientation[1] + p.Z * orientation[2];
            }
        }
    }
}