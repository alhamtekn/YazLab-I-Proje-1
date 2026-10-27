namespace Lumen.EditorTools.SceneBuilding
{
    /// <summary>
    /// Sahnenin bir bölümünü kuran sınıf. Her geliştirici kendi bölüm sınıfında çalışır;
    /// sahne dosyası bu bölümlerden yeniden üretildiği için .unity dosyasında çakışma oluşmaz.
    /// Bölümler Order değerine göre küçükten büyüğe çalışır.
    /// </summary>
    public interface ISceneSection
    {
        int Order { get; }

        void Build(SceneBuildContext context);
    }
}
