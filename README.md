MagicTap

## Proje Hakkında

**MagicTap**, oyuncunun üzerine tıkladığı yön oklarıyla işaretlenmiş küpleri belirli bir hamle hakkı içinde temizlemeye çalıştığı bir Unity 3B bulmaca oyunudur.  
Oyuncu her tıklamada ilgili küpü ok yönünde hareket ettirir; tüm küpler temizlendiğinde bir sonraki seviyeye geçilir.

## Özellikler

- **Küçük bir seviye havuzu**: Hamle limiti ve küp sayısı kademeli olarak artar.  
- **Yönlendirme okları**: Her küpün üzerinde hareket yönünü gösteren bir ok var.  
- **Hamle sayacı**: Ekranda kalan hamle sayısı anlık güncellenir.  
- **Seviye yönetimi**: Başarı/kaybetme ekranları, bir sonraki/yeniden dene butonları.  
- **Modüler mimari**:  
  - Factory pattern ile küp üretimi  
  - Singleton pattern ile oyun durumu yönetimi  
  - Observer pattern ile hamle ve temizleme olayları  
- **Performans iyileştirmeleri**: Object pooling ile küp nesnelerinin tekrar kullanımı.  
- **Basit animasyonlar**: DOTween kullanılarak küp hareketleri ve UI geçişleri.
