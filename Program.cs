using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            int width = 640;
            int height = 480;

            Scene scene = new Scene();

            // eye, center, up
            Camera camera = Camera.LookAt(new Vector(0, 50, 1), new Vector(4, 20, 5), new Vector(0, 0, 1), 50);

            // Light
            scene.Add(Sphere.NewSphere(new Vector(0, 0, 4), 0.5, Material.LightMaterial(Color.White, 20)));

            // Plane
            Plane plane = Plane.NewPlane(new Vector(0, 0, 0), new Vector(0, 0, 1), Material.SpecularMaterial(new Color(1.0, 1.0, 1.0), 1));
            scene.Add(plane);

            // Sphere
            Material mat1 = Material.DiffuseMaterial(new Color(1.0, 1.0, 1.0));
            Material mat2 = Material.GlossyMaterial(new Color(1.0, 0.0, 1.0), 1.4, Util.Radians(30));
            Material mat3 = Material.MetallicMaterial(new Color(1.0, 1.0, 0.0), 1.5, 1.0);
            Material mat4 = Material.SpecularMaterial(new Color(0.0, 1.0, 1.0), 1.0);
            Material mat5 = Material.TransparentMaterial(new Color(1.0, 1.0, 1.0), 2, Util.Radians(20), 1);
            Material mat6 = Material.ClearMaterial(0.5, 0.5);
            Material lightmaterial = Material.LightMaterial(Color.White, 20);

            // Spheres
            scene.Add(Sphere.NewSphere(new Vector(-6, 0, 1), 1, mat1)); // Diffuse
            scene.Add(Sphere.NewSphere(new Vector(-4, 0, 1), 1, mat2)); // Glossy
            scene.Add(Sphere.NewSphere(new Vector(-2, 0, 1), 1, mat3)); // Metallic
            scene.Add(Sphere.NewSphere(new Vector(0, 0, 1), 1, mat4));  // Specular
            scene.Add(Sphere.NewSphere(new Vector(2, 0, 1), 1, mat5));  // Transparent
            scene.Add(Sphere.NewSphere(new Vector(4, 0, 1), 1, mat6));  // Clear
            
            Sampler sampler = DefaultSampler.NewSampler(4, 4);
            Renderer renderer = Renderer.NewRenderer(scene, camera, sampler, width, height);

            renderer.AdaptiveSamples = 50;
            renderer.FireflySamples = 32;

            renderer.IterativeRender("./out.png", 2);
        }
    }
}