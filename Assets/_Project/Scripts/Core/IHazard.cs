namespace Lumen.Core
{
    /// <summary>
    /// Sensörlerin algılayabildiği tehlikeler. "Dangerous" hem uyarı hem aktif aşamayı kapsar;
    /// böylece ajan tehlike tam açılmadan önce tepki verebilir.
    /// </summary>
    public interface IHazard
    {
        bool IsDangerous { get; }
    }

    /// <summary>
    /// Bir tehlikeye yakalandığında geri konumlanabilen varlıklar.
    /// </summary>
    public interface IHazardVictim
    {
        void OnHazardHit(UnityEngine.Vector3 resetPosition);
    }
}
