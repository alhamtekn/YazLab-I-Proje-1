# Görevler

Teslim: **6 Kasım Cuma 17:00**. Bu dosya güncel görev listesidir; bir görev değişince en alttaki **Değişiklik kaydı**na tarihli bir satır eklenir. `git pull` sonrası önce bu dosyaya bakın.

- **Geliştirici 1 — Onur (OnurTOPBAS):** eşlikçi ve seviye
- **Geliştirici 2 — alhamtekn:** Ollama NPC, sohbet arayüzü, oyun durumu

## Çalışma kuralları

- Herkes kendi dalında çalışır, `main`'e PR ile girer. Diğer kişi PR'ı inceler.
- `Assets/_Project/Scenes/Lumen.unity` **elle düzenlenmez.** Sahneye bir şey eklemek için `Assets/_Project/Editor/SceneBuilding/Sections/` altına yeni bir `ISceneSection` sınıfı yazılır (otomatik bulunur), ardından **Lumen → Sahneyi Yeniden Oluştur** çalıştırılır.
- Her gün en az 2-3 anlamlı commit. Commit sayıları dengeli olmazsa puan kaybedilir; şu an tüm commit'ler alhamtekn'de, bu yüzden Onur'un düzenli commit atması kritik.
- Commit e-postası GitHub hesabında doğrulanmış olmalı (`git config user.email`).
- Pazartesi ve Perşembe `git pull origin main`.

## Mevcut durum (27 Ekim)

1. bölüm tamam ve testli: oyuncu, NavMesh + FSM eşlikçi Lumi, `RaycastSensor`, tuzak odası, şalter ve köprü. PlayMode testleri 30/30 başarılı. Ayrıntı README'de.

## Onur — eşlikçi ve seviye

| Tarih | Görev |
| --- | --- |
| 27 Eki Sal | Daveti kabul et, klonla, `git config user.name/user.email` ayarla. Unity 6000.6.7f1 ile aç, **Lumen → Sahneyi Yeniden Oluştur**, `Lumen.unity`'de Play. `Companion` ve `Level` kodunu oku. `feature/ara-bolge` dalını aç. |
| 28 Eki Çar | `CrystalPickup.cs` (3 kristal) ve `ILoreJournal`'ı uygulayan `LoreJournal.cs`. |
| 29 Eki Per | Meşaleler, yazıtlar, karanlık ortam ışıklandırması. |
| 30 Eki Cum | Ara bölgeyi sahneye ekleyen `MiddleZoneSection` (köprüden sonra, NPC kapısından önce). |
| 31 Eki Cmt | Kristaller toplanınca NPC bölgesine geçişin açılması; ara bölge için PlayMode testi. |
| 1 Kas Paz | `Environment_Prefab`, `Obstacle_Prefab`; Lumi'nin ara bölgede de takip ettiğini kontrol et. |
| 2 Kas Pzt | PR → `main`. alhamtekn'in PR'ını incele. |
| 3 Kas Sal | Ana sahnede uçtan uca test (engel → ara bölge → NPC → son). |
| 4 Kas Çar | Eşlikçi testlerini kendi bilgisayarında yeniden çalıştır, sonuçları ve ekran görüntülerini ekle. |
| 5 Kas Per | README: eşlikçi bölümü (FSM diyagramı, sensör, test tablosu) kendi anlatımınla gözden geçir. |
| 6 Kas Cum | Build, son kontrol, teslim (sabahtan). |

## alhamtekn — Ollama NPC ve arayüz

| Tarih | Görev |
| --- | --- |
| 27 Eki Sal | `feature/llm-npc` dalı. Ollama'yı kur, model indir (`llama3.2:3b` ya da `phi3:mini`), `curl` ile dene. |
| 28 Eki Çar | `ILLMService` arayüzü + `OllamaService.cs` (UnityWebRequest, async POST, timeout). |
| 29 Eki Per | `NPCResponse` modeli + `NPCResponseParser.cs`; geçersiz JSON güvenli varsayılana düşer. |
| 30 Eki Cum | Hata yönetimi: sunucu kapalı, zaman aşımı, bozuk yanıt. |
| 31 Eki Cmt | Sohbet UI: giriş alanı, mesaj geçmişi, "Düşünüyor..." göstergesi, istek sürerken giriş kilidi. |
| 1 Kas Paz | Arven system prompt'u, `GameStateManager.cs` (kapı açılır, oyun biter), NPC sahne bölümü. |
| 2 Kas Pzt | PR → `main`. Onur'un PR'ını incele. |
| 3 Kas Sal | Uçtan uca akış testi. |
| 4 Kas Çar | En az 5 diyalog testi (prompt injection dahil) + Ollama kapalı testi, ekran görüntüleri. |
| 5 Kas Per | README: LLM sequence diyagramı, system prompt, JSON şeması, hata stratejisi. |
| 6 Kas Cum | Build, Ollama kapalıyken build testi, teslim. |

## Değişiklik kaydı

- **27 Eki:** NavMesh + FSM kesinleşti, ML-Agents yok. 1. bölüm (Lumi, şalter, köprü, tuzaklar, testler) hazır olduğu için Onur'un 28-31 Ekim görevleri ara bölgeye kaydırıldı. Eski plandaki "`81a18bb`'den Core'u geri getir" adımı artık gereksiz; Core `main`'de.
