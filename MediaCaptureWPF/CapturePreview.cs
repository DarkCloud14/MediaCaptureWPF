using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Interop;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using MediaCaptureWPF.Properties;

namespace MediaCaptureWPF
{
  public class CapturePreview : D3DImage, IDisposable
  {
    dynamic m_preview;
    MediaCapture m_capture;
    uint m_width;
    uint m_height;
    bool m_disposed;

    public CapturePreview(MediaCapture capture)
    {
      const string mediaCaptureWpfNativeAssemblyDllName = "MediaCaptureWPF.Native.dll";
      var mediaCaptureAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.ManifestModule.Name.Equals(mediaCaptureWpfNativeAssemblyDllName, StringComparison.InvariantCultureIgnoreCase));

      // We only do this if the assembly wasn't loaded yet.
      if (mediaCaptureAssembly == null)
      {
        var tempAssemblyFile = Path.Combine(Path.GetTempPath(), "MediaCaptureWPFNativeFiles", Environment.Is64BitProcess ? "x64" : "x86", mediaCaptureWpfNativeAssemblyDllName);
        var rawBytesOfTempAssemblyFile = new byte[1];

        if (File.Exists(tempAssemblyFile))
        {
          rawBytesOfTempAssemblyFile = File.ReadAllBytes(tempAssemblyFile);
        }
        else
        {
          if (!Directory.Exists(tempAssemblyFile))
          {
            Directory.CreateDirectory(Path.GetDirectoryName(tempAssemblyFile));
          }
        }

        // If the files are not equal try to overwrite the file. If it doesn't work (file could be locked) the existing assembly is loaded.
        if (!StructuralComparisons.StructuralEqualityComparer.Equals(rawBytesOfTempAssemblyFile, Environment.Is64BitProcess ? Resources.MediaCaptureWPFNative_x64 : Resources.MediaCaptureWPFNative_x86))
        {
          try
          {
            File.WriteAllBytes(tempAssemblyFile, Environment.Is64BitProcess ? Resources.MediaCaptureWPFNative_x64 : Resources.MediaCaptureWPFNative_x86);
          }
          catch (Exception ex)
          {
          }
        }

        mediaCaptureAssembly = Assembly.LoadFile(tempAssemblyFile);
      }

      var props = (VideoEncodingProperties)capture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
      m_width = props.Width;
      m_height = props.Height;

      var capturePreviewNativeType = mediaCaptureAssembly.GetType("MediaCaptureWPF.Native.CapturePreviewNative", true);
      m_preview = Activator.CreateInstance(capturePreviewNativeType, this, m_width, m_height);
      m_capture = capture;
    }

    public async Task StartAsync()
    {
      var profile = new MediaEncodingProfile
      {
        Audio = null,
        Video = VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Rgb32, m_width, m_height),
        Container = null
      };

      await m_capture.StartPreviewToCustomSinkAsync(profile, (IMediaExtension)m_preview.MediaSink);
    }

    public async Task StopAsync()
    {
      await m_capture.StopPreviewAsync();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (m_disposed)
        return;

      if (disposing)
      {
        // Free any other managed objects here
      }

      // Free any unmanaged objects here
      m_preview?.Dispose();
      m_disposed = true;
    }
  }
}
