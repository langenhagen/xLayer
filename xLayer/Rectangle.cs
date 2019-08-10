using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

using Device    = SlimDX.Direct3D11.Device;
using Resource  = SlimDX.Direct3D11.Resource;
using D3DBuffer = SlimDX.Direct3D11.Buffer;

namespace xLayer
{
    class Rectangle : Entity
    {

        /////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTOR & DESTRUCTOR

        /// <summary>
        /// TODO doc
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="span"></param>
        /// <param name="device"></param>
        /// <param name="context"></param>
        /// <param name="vertexShader"></param>
        /// <param name="vsSignature"></param>
        /// <param name="pixelShader"></param>
        /// <param name="resourceProviders"></param>
        public Rectangle(
            Vector2 origin, 
            Vector2 span,
            EntityDescription desc)
            : base( desc)
        {

            // create rect vertices
            int vertexLength  = Vector3.SizeInBytes;    // should be 12:    3 * 4 bytes
            int uvCoordLength = Vector2.SizeInBytes;    // should be 8:     2 * 4 bytes

            DataStream data  = new DataStream( (vertexLength+uvCoordLength) * 4, true, true);

            origin.X = origin.X*2 -1;
            origin.Y = 1 - origin.Y*2;

            float originPlusSpanX = origin.X + span.X*2;
            float originPlusSpanY = origin.Y - span.Y*2;
            
            // write 4 times: position vector & uv-coord vector ...
            data.Write(new Vector3(origin.X,            origin.Y,            0));       // vertex
            data.Write(new Vector2(0, 0));                                              // UV coord
            data.Write(new Vector3(originPlusSpanX,     origin.Y,            0));       // vertex
            data.Write(new Vector2(1, 0));                                              // UV coord 
            data.Write(new Vector3(origin.X,            originPlusSpanY,     0));       // ...
            data.Write(new Vector2(0, 1));
            data.Write(new Vector3(originPlusSpanX,     originPlusSpanY,     0));
            data.Write(new Vector2(1, 1));
            data.Position = 0;
            

            // create the vertex layout and buffer
            var elements = new[] { new InputElement("POSITION",  0, Format.R32G32B32_Float,           0,  0), 
                                   new InputElement("TEXTCOORD", 0, Format.R32G32_Float,    vertexLength, 0) };
            
            InputLayout = new InputLayout(Device, VertexShaderSignature, elements);


            VertexBuffer = new D3DBuffer(   Device,
                                            data,
                                            (int)data.Length,
                                            ResourceUsage.Default,
                                            BindFlags.VertexBuffer,
                                            CpuAccessFlags.None,
                                            ResourceOptionFlags.None,
                                            0);

            VertexBufferBinding = new VertexBufferBinding(VertexBuffer, vertexLength+uvCoordLength, 0);
            
            data.Close();
            data.Dispose();
        }



        /////////////////////////////////////////////////////////////////////////////////
        // METHODS

        public override int GetNumVertices()
        {
            return 4;
        }

        /////////////////////////////////////////////////////////////////////////////////
        // GETTERS & SETTERS
    }
}
