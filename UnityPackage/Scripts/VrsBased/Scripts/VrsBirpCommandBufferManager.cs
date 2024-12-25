using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// A simple template command buffer.
/// </summary>
namespace FoveatedRenderingVRS_BIRP
{
    public class VrsBirpCommandBufferManager
    {
        public delegate void BufferAction(CommandBuffer cmd);

        private class BufferObject
        {
            string bufferName;
            CameraEvent cameraEvent;
            CommandBuffer commandBuffer;

            public BufferObject(string name, CameraEvent camEvent, params BufferAction[] actions)
            {
                bufferName = name;
                cameraEvent = camEvent;

                commandBuffer = new CommandBuffer
                {
                    name = bufferName
                };

                foreach (var action in actions)
                {
                    action(commandBuffer);
                }
            }

            public void Activate(Camera cam)
            {
                cam.AddCommandBuffer(cameraEvent, commandBuffer);
            }

            public void Deactivate(Camera cam)
            {
                cam.RemoveCommandBuffer(cameraEvent, commandBuffer);
            }
        }

        private List<BufferObject> buffers = new List<BufferObject>();

        /// <summary>
        /// Adds a new command buffer with specified actions.
        /// </summary>
        public void AddCommandBuffer(string name, CameraEvent camEvent, params BufferAction[] actions)
        {
            var bufferObject = new BufferObject(name, camEvent, actions);
            buffers.Add(bufferObject);
        }

        /// <summary>
        /// Clears all command buffers from the manager.
        /// </summary>
        public void ClearAllBuffers()
        {
            buffers.Clear();
        }

        /// <summary>
        /// Enables all command buffers for the specified camera.
        /// </summary>
        public void EnableBuffers(Camera cam)
        {
            foreach (var buffer in buffers)
            {
                buffer.Activate(cam);
            }
        }

        /// <summary>
        /// Disables all command buffers for the specified camera.
        /// </summary>
        public void DisableBuffers(Camera cam)
        {
            foreach (var buffer in buffers)
            {
                buffer.Deactivate(cam);
            }
        }
    }
}
