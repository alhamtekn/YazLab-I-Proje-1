# Görevler

Teslim: **6 Kasım Cuma 17:00**. Bu dosya güncel görev listesidir; bir görev değişince en alttaki **Değişiklik kaydı**na tarihli bir satır eklenir. `git pull` sonrası önce bu dosyaya bakın.

- **Geliştirici 1 — Onur (OnurTOPBAS):** eşlikçi ve seviye (kurulum tamam)
- **Geliştirici 2 — alhamtekn:** Ollama NPC, sohbet arayüzü, oyun durumu

## Çalışma kuralları

- Herkes kendi dalında çalışır, `main`'e PR ile girer. Diğer kişi PR'ı inceler.
- `Assets/_Project/Scenes/Lumen.unity` **elle düzenlenmez.** Sahneye bir şey eklemek için `Assets/_Project/Editor/SceneBuilding/Sections/` altına yeni bir `ISceneSection` sınıfı yazılır (otomatik bulunur), ardından **Lumen → Sahneyi Yeniden Oluştur** çalıştırılır.
- Her gün en az 2-3 anlamlı commit. Commit sayıları dengeli olmazsa puan kaybedilir; şu an tüm commit'ler alhamtekn'de, bu yüzden Onur'un düzenli commit atması kritik.
- Commit e-postası GitHub hesabında doğrulanmış olmalı (`git config user.email`).
- Pazartesi ve Perşembe `git pull origin main`.

## Mevcut durum (6 Kasım, teslim günü)

Oyun baştan sona tamam: ana menü, hikâye koridoru ve Lumi ile tanışma, 1. bölüm, Lumi ile geçilen kristal odaları, Ollama destekli Arven, kutsal bölüm (İkiz Mühür, Işığın Yolu), bitiş ekranı. 53 PlayMode testinin 53'ü geçiyor; gerçek Ollama ile diyalog testi `TestResults/dialogue_results.md`'de. Rapor README'de. 17 PR `main`'e alındı.

Teslimden önce kalan: son `main` ile **Lumen → Windows Build Al**, `Builds/Lumen/Lumen.exe`'nin Ollama açıkken ve kapalıyken denenmesi.

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
- **28 Eki – 1 Kas:** Ara bölge (kristaller, yazıtlar, kristal kapısı), Ollama NPC Arven, sohbet arayüzü ve oyun durumu tamamlandı (PR #1–#5).
- **1–2 Kas:** Prompt injection savunması (filtre, +10 sınırı, bölümler bitmeden 29 tavanı, kapıyı oyun kuralı açar); kristal odaları; ayrıştırıcı testleri (PR #6, #7).
- **2–3 Kas:** Ana menü, birinci şahıs kamera, gerilim müziği, kutsal bölüm; tapınak dokuları; uçtan uca test (PR #8–#10).
- **3–4 Kas:** Giriş koridoru ve Lumi ile tanışma, köprü dokusu, lav, tavan, Lumi ile geçilen kristal odası parkurları; mini harita, tuşlar paneli, duvar arkası etkileşim, .exe build (PR #11–#14).
- **4–5 Kas:** Hikâye metinleri, ses efektleri, kutsal bölüm müziği, bitiş ekranı; test sonuçları ve ekran görüntüleri; Arven'in göz rengi, güven çubuğu, harf harf cevap, nişangah, yeniden başlatma (PR #15–#17).
- **6 Kas:** Gerçek Ollama ile diyalog testi, README raporu son haline getirildi, son build.
