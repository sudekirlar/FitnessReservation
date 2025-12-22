import React, { useState, useEffect, useMemo } from 'react';
import { 
  Dumbbell, 
  Wind, 
  Activity, 
  Flame, 
  Music, 
  Calendar, 
  ShoppingCart, 
  User, 
  LogOut, 
  CreditCard,
  CheckCircle,
  AlertCircle,
  Menu,
  X,
  Crown,
  GraduationCap,
  Clock,
  Loader,
  ChevronRight,
  Gem,       
  Sparkles,  
  Trophy     
} from 'lucide-react';

// --- API YAPILANDIRMASI ---
const API_BASE = 'http://localhost:7001'; 

// --- HELPER FONKSİYONLAR ---
const getMembershipType = (user) => {
  if (!user) return 'Standard';
  const isUtkVip = localStorage.getItem('is_utk_vip') === 'true';
  const type = user.membershipType;
  
  if (isUtkVip && (type === 2 || type === 'Premium')) return 'UTK-VIP';
  if (type === 2 || type === 'Premium') return 'Premium';
  if (type === 1 || type === 'Student') return 'Student';
  return 'Standard';
};

// --- TEMA YAPILANDIRMASI ---
const THEMES = {
  default: {
    name: 'dark',
    bg: 'bg-zinc-950',
    navBg: 'bg-zinc-900/90 backdrop-blur-md border-b border-zinc-800',
    cardBg: 'bg-zinc-900 border border-zinc-800',
    textMain: 'text-zinc-50',
    textSub: 'text-zinc-400',
    accent: 'text-emerald-400',
    accentBg: 'bg-emerald-500',
    button: 'bg-emerald-600 hover:bg-emerald-500 text-white',
    buttonOutline: 'border border-emerald-600 text-emerald-400 hover:bg-emerald-900/20',
    badge: 'bg-zinc-800 text-zinc-300'
  },
  premium: {
    name: 'premium',
    bg: 'bg-[#0f0518]', 
    navBg: 'bg-[#1a0b2e]/90 backdrop-blur-md border-b border-purple-900/50',
    cardBg: 'bg-[#1a0b2e] border border-purple-900/30',
    textMain: 'text-purple-50',
    textSub: 'text-purple-300/70',
    accent: 'text-fuchsia-400',
    accentBg: 'bg-fuchsia-500',
    button: 'bg-fuchsia-700 hover:bg-fuchsia-600 text-white shadow-[0_0_15px_rgba(192,38,211,0.3)]',
    buttonOutline: 'border border-fuchsia-600 text-fuchsia-400 hover:bg-fuchsia-900/20',
    badge: 'bg-purple-900/50 text-fuchsia-200 border border-purple-700'
  },
  student: {
    name: 'student',
    bg: 'bg-[#020617]', 
    navBg: 'bg-[#0f172a]/90 backdrop-blur-md border-b border-blue-900/30',
    cardBg: 'bg-[#0f172a] border border-blue-900/30',
    textMain: 'text-blue-50',
    textSub: 'text-slate-400',
    accent: 'text-sky-400',
    accentBg: 'bg-sky-500',
    button: 'bg-blue-600 hover:bg-blue-500 text-white shadow-[0_0_15px_rgba(37,99,235,0.3)]',
    buttonOutline: 'border border-blue-600 text-blue-400 hover:bg-blue-900/20',
    badge: 'bg-blue-900/50 text-blue-200 border border-blue-800'
  },
  'utk-vip': { // ULTRA LÜKS ALTIN TEMA
    name: 'utk-vip',
    bg: 'bg-gradient-to-br from-black via-[#1a1200] to-black', 
    navBg: 'bg-black/90 backdrop-blur-xl border-b border-yellow-600/50',
    cardBg: 'bg-black/60 border border-yellow-500/40 shadow-[0_0_25px_rgba(234,179,8,0.15)]',
    textMain: 'text-yellow-50',
    textSub: 'text-yellow-200/60',
    accent: 'text-yellow-400',
    accentBg: 'bg-gradient-to-r from-yellow-400 via-amber-500 to-yellow-600',
    button: 'bg-gradient-to-r from-yellow-600 via-amber-500 to-yellow-600 hover:from-yellow-500 hover:to-amber-400 text-black font-bold shadow-[0_0_20px_rgba(234,179,8,0.4)] tracking-widest',
    buttonOutline: 'border border-yellow-500 text-yellow-400 hover:bg-yellow-900/30 shadow-[0_0_10px_rgba(234,179,8,0.2)]',
    badge: 'bg-gradient-to-r from-yellow-900/80 to-amber-900/80 text-yellow-100 border border-yellow-400 shadow-[0_0_15px_rgba(234,179,8,0.3)]'
  }
};

// --- STATİK DATA ---
const SPORTS = [
  { id: 'Pilates', title: 'Pilates', image: 'https://images.unsplash.com/photo-1518611012118-696072aa579a?q=80&w=800&auto=format&fit=crop', Icon: Wind, desc: 'Esneklik ve denge.' },
  { id: 'Yoga', title: 'Yoga', image: 'https://images.unsplash.com/photo-1575052814086-f385e2e2ad1b?q=80&w=800&auto=format&fit=crop', Icon: Activity, desc: 'Zihinsel ve fiziksel huzur.' },
  { id: 'Spinning', title: 'Spinning', image: 'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?q=80&w=800&auto=format&fit=crop', Icon: Dumbbell, desc: 'Yüksek tempo kardiyo.' },
  { id: 'HIIT', title: 'HIIT', image: 'https://images.unsplash.com/photo-1601422407692-ec4eeec1d9b3?q=80&w=800&auto=format&fit=crop', Icon: Flame, desc: 'Maksimum yağ yakımı.' },
  { id: 'Zumba', title: 'Zumba', image: 'https://images.unsplash.com/photo-1524594152303-9fd13543fe6e?q=80&w=800&auto=format&fit=crop', Icon: Music, desc: 'Dans ve eğlence.' }
];

// Mock Data
const generateMockSessions = (date) => {
  return [
    { sessionId: 'mock-1', instructorName: 'Zeynep Yılmaz', startsAtUtc: new Date(new Date(date).setHours(10, 0)).toISOString(), reservedCount: 5, capacity: 20, isPeak: false, occupancyLevel: 'Low', price: { finalPrice: 150 } },
    { sessionId: 'mock-2', instructorName: 'Caner Erkin', startsAtUtc: new Date(new Date(date).setHours(14, 0)).toISOString(), reservedCount: 18, capacity: 20, isPeak: false, occupancyLevel: 'High', price: { finalPrice: 150 } },
    { sessionId: 'mock-3', instructorName: 'Selin Demir', startsAtUtc: new Date(new Date(date).setHours(9, 0)).toISOString(), reservedCount: 2, capacity: 15, isPeak: false, occupancyLevel: 'Low', price: { finalPrice: 180 } },
    { sessionId: 'mock-4', instructorName: 'Mert Yıldız', startsAtUtc: new Date(new Date(date).setHours(19, 0)).toISOString(), reservedCount: 15, capacity: 15, isPeak: true, occupancyLevel: 'High', price: { finalPrice: 220 } }
  ];
};

const getInstructorImage = (name) => {
  return `https://i.pravatar.cc/150?u=${encodeURIComponent(name)}`;
};

const handleImageError = (e, fallbackText) => {
  e.target.onerror = null; 
  e.target.src = `https://placehold.co/800x600/1f2937/ffffff?text=${fallbackText || 'Görsel+Yok'}`;
};

export default function FitLifeApp() {
  const [currentPage, setCurrentPage] = useState('home');
  const [selectedSport, setSelectedSport] = useState(null);
  const [cart, setCart] = useState([]); 
  const [user, setUser] = useState(null); 
  const [notification, setNotification] = useState(null);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [loading, setLoading] = useState(false);

  const userType = getMembershipType(user);
  const theme = THEMES[userType.toLowerCase()] || THEMES.default;

  const showNotification = (msg, type = 'success') => {
    setNotification({ msg, type });
    setTimeout(() => setNotification(null), 4000);
  };

  const navigateTo = (page) => {
    setCurrentPage(page);
    setIsMobileMenuOpen(false);
    window.scrollTo(0, 0);
  };

  const apiCall = async (endpoint, method = 'GET', body = null) => {
    try {
      const options = {
        method,
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include', 
      };
      if (body) options.body = JSON.stringify(body);

      const res = await fetch(`${API_BASE}${endpoint}`, options);
      
      if (res.status === 401) {
        if (endpoint === '/me') return null; 
        throw new Error('Oturum süresi doldu veya yetkisiz işlem.');
      }

      if (!res.ok) {
        const errText = await res.text();
        let errData = {};
        try { errData = JSON.parse(errText); } catch(e) {}
        throw new Error(errData.error || errData.message || errText || `Hata: ${res.status}`);
      }
      
      if (res.status === 204) return null;
      return await res.json();
    } catch (err) {
      if (endpoint !== '/me') { 
         showNotification(err.message, 'error');
      }
      throw err;
    }
  };

  useEffect(() => {
    const checkUser = async () => {
      try {
        const me = await apiCall('/me');
        if (me) setUser(me);
      } catch (e) { }
    };
    checkUser();
  }, []);

  // --- AKSİYONLAR ---

  const handleLogin = async (username, password) => {
    setLoading(true);
    try {
      const res = await apiCall('/auth/login', 'POST', { username, password });
      setUser(res); 
      showNotification(`Hoşgeldin ${res.username}!`);
      if (cart.length > 0) navigateTo('cart');
      else navigateTo('home');
    } catch (e) { } finally {
      setLoading(false);
    }
  };

  const handleRegister = async (formData) => {
    setLoading(true);
    try {
      // 1. KODU TEMİZLE VE BÜYÜT (Türkçe karakter düzeltmesiyle)
      // 'ı' harfini 'i' yapıyoruz ki 'UTK-VIP' olsun.
      let codeToSend = formData.membershipCode || '';
      codeToSend = codeToSend.replace(/ı/g, 'i').replace(/İ/g, 'I'); 
      codeToSend = codeToSend.trim().toUpperCase();

      // 2. ÜYELİK TİPİNİ BELİRLE
      // Eğer kod UTK-VIP ise, Backend'e "Premium" gönder
      // Değilse, formdaki tipi (Student/Premium/Standard) gönder
      let typeToSend = formData.membershipType;
      
      // UTK-VIP kontrolü: Hem koda hem UI tipine bakıyoruz, garanti olsun
      if (codeToSend === 'UTK-VIP' || formData.membershipType === 'UTK-VIP') {
        typeToSend = 'Premium'; // Backend için tip her zaman Premium
        codeToSend = 'UTK-VIP'; // Kod da her zaman UTK-VIP
      }

      // Backend'e gidecek final obje
      const body = {
        username: formData.username,
        password: formData.password,
        membershipType: typeToSend, 
        membershipCode: codeToSend || null
      };

      console.log("Backend'e gönderilen istek:", body); // Konsoldan kontrol edebilirsin

      await apiCall('/auth/register', 'POST', body);
      
      // UTK-VIP ise yerel depolamaya özel işaret koy
      if (codeToSend === 'UTK-VIP') {
        localStorage.setItem('is_utk_vip', 'true');
      } else {
        localStorage.removeItem('is_utk_vip');
      }

      await handleLogin(formData.username, formData.password);
    } catch (e) { 
      // Hata zaten apiCall içinde gösteriliyor
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      await apiCall('/auth/logout', 'POST');
      setUser(null);
      setCart([]);
      localStorage.removeItem('is_utk_vip'); 
      navigateTo('home');
      showNotification('Çıkış yapıldı.');
    } catch (e) {}
  };

  const handleReserve = async (sessionId, price, instructor, date, time) => {
    const newItem = { cartId: Date.now(), sessionId, instructorName: instructor, totalPrice: price, sportName: selectedSport.title, date, time };
    setCart([...cart, newItem]);
    showNotification('Ders sepete eklendi!');
    navigateTo('cart');
  };

  const handlePurchase = async () => {
    if (!user) {
      showNotification('Satın alma işlemi için lütfen giriş yapın veya üye olun.', 'error');
      navigateTo('signup');
      return;
    }
    setLoading(true);
    try {
      const realItems = cart.filter(item => !item.sessionId.startsWith('mock-'));
      for (const item of realItems) {
        await apiCall('/reservations', 'POST', { sessionId: item.sessionId });
      }
      showNotification('Ödeme başarıyla alındı! İyi dersler.', 'success');
      setCart([]);
      navigateTo('home');
    } catch (e) { } finally {
      setLoading(false);
    }
  };

  // --- SAYFALAR ---

  const Navbar = () => (
    <nav className={`${theme.navBg} sticky top-0 z-50 shadow-2xl transition-colors duration-500`}>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-20">
          <div 
            className={`flex items-center cursor-pointer font-black text-3xl tracking-tighter ${theme.accent} transition-colors duration-300`}
            onClick={() => navigateTo('home')}
            style={{ fontFamily: 'Arial Black, sans-serif' }}
          >
            {userType === 'UTK-VIP' && <Gem className="mr-2 w-8 h-8 animate-pulse text-yellow-400" />} 
            {!userType.includes('UTK') && <Dumbbell className="mr-2 w-8 h-8" />} 
            GYM GYM'E
          </div>
          
          <div className="hidden md:flex items-center space-x-4">
            {user && (
              <div className={`flex items-center text-sm px-4 py-1.5 rounded-full border ${userType === 'Premium' ? 'bg-purple-900/30 border-purple-500/50' : userType === 'Student' ? 'bg-blue-900/30 border-blue-500/50' : userType === 'UTK-VIP' ? theme.badge : 'bg-zinc-800 border-zinc-700'}`}>
                {userType === 'Premium' && <Crown className="w-4 h-4 text-yellow-400 mr-2" />}
                {userType === 'Student' && <GraduationCap className="w-4 h-4 text-blue-400 mr-2" />}
                {userType === 'UTK-VIP' && <Sparkles className="w-4 h-4 text-yellow-200 mr-2 animate-spin-slow" />}
                
                <span className={`${theme.textMain} font-bold mr-2 uppercase tracking-wide flex items-center`}>
                  {userType === 'UTK-VIP' 
                    ? <span className="text-[10px] tracking-widest bg-clip-text text-transparent bg-gradient-to-r from-yellow-200 via-yellow-400 to-yellow-200 font-black">PREMIUM PLATINUM DIAMOND ELITE PLUS PRO MAX LIMITED EDITION</span> 
                    : userType}
                </span>
                <span className={`${theme.textSub}`}>| {user.username}</span>
              </div>
            )}

            <button onClick={() => navigateTo('cart')} className={`flex items-center px-4 py-2 rounded-lg ${theme.textSub} hover:${theme.textMain} hover:bg-white/5 transition relative`}>
              <ShoppingCart className="w-5 h-5 mr-2" />
              Classlarım
              {cart.length > 0 && <span className={`absolute top-0 right-0 ${theme.accentBg} text-white text-[10px] font-bold rounded-full w-5 h-5 flex items-center justify-center transform translate-x-1 -translate-y-1`}>{cart.length}</span>}
            </button>

            {user ? (
              <button onClick={() => navigateTo('profile')} className={`flex items-center px-4 py-2 rounded-lg ${theme.textSub} hover:${theme.textMain} hover:bg-white/5 transition`}>
                <User className="w-5 h-5 mr-2" />
                Profil
              </button>
            ) : (
              <button onClick={() => navigateTo('signup')} className={`${theme.button} px-6 py-2 rounded-lg font-bold transition transform hover:scale-105`}>
                Üye Ol
              </button>
            )}
          </div>
          <div className="md:hidden flex items-center">
             <button onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)} className={`${theme.textMain} p-2`}>
                {isMobileMenuOpen ? <X size={24} /> : <Menu size={24} />}
             </button>
          </div>
        </div>
      </div>
    </nav>
  );

  const HomePage = () => (
    <div className="max-w-7xl mx-auto px-4 py-12">
      <div className="text-center mb-16 space-y-4">
        <h1 className={`text-5xl md:text-7xl font-black ${theme.textMain} tracking-tighter uppercase`}>
          Limit <span className="text-transparent bg-clip-text bg-gradient-to-r from-emerald-400 to-cyan-500">Yok</span>
        </h1>
        <p className={`text-xl ${theme.textSub} max-w-2xl mx-auto`}>GYM GYM'E ile potansiyelini açığa çıkar.</p>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
        {SPORTS.map((sport) => (
          <div key={sport.id} onClick={() => { setSelectedSport(sport); navigateTo('classes'); }} className={`group ${theme.cardBg} rounded-2xl overflow-hidden hover:shadow-[0_0_30px_rgba(0,0,0,0.5)] transition-all duration-500 cursor-pointer relative`}>
            <div className="relative h-64 overflow-hidden">
              <img 
                src={sport.image} 
                alt={sport.title} 
                onError={(e) => handleImageError(e, sport.title)}
                className="w-full h-full object-cover group-hover:scale-110 group-hover:rotate-1 transition-transform duration-700 opacity-80 group-hover:opacity-100" 
              />
              <div className="absolute inset-0 bg-gradient-to-t from-black via-black/20 to-transparent opacity-90" />
              <div className="absolute bottom-4 left-4 right-4 translate-y-2 group-hover:translate-y-0 transition-transform duration-300">
                <div className="flex items-center space-x-2 mb-2">
                  <div className={`${theme.accentBg} p-2 rounded-lg text-white`}>
                    <sport.Icon className="w-6 h-6" />
                  </div>
                  <h3 className={`text-2xl font-bold ${theme.textMain}`}>{sport.title}</h3>
                </div>
                <p className={`${theme.textSub} text-sm line-clamp-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300`}>{sport.desc}</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  const ClassesPage = () => {
    const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
    const [sessions, setSessions] = useState([]);
    const [fetching, setFetching] = useState(false);

    useEffect(() => {
      if (!selectedSport || !date) return;
      const fetchSessions = async () => {
        setFetching(true);
        try {
          if (user) {
            const fromDate = new Date(date);
            const toDate = new Date(date);
            toDate.setHours(23, 59, 59);
            const qs = new URLSearchParams({ sport: selectedSport.id, from: fromDate.toISOString(), to: toDate.toISOString() });
            const data = await apiCall(`/sessions?${qs.toString()}`);
            setSessions(data);
          } else {
            await new Promise(r => setTimeout(r, 500));
            setSessions(generateMockSessions(date));
          }
        } catch (e) { setSessions([]); } finally { setFetching(false); }
      };
      fetchSessions();
    }, [date, selectedSport, user]);

    const groupedSessions = useMemo(() => {
        return sessions.reduce((acc, session) => {
            if (!acc[session.instructorName]) acc[session.instructorName] = [];
            acc[session.instructorName].push(session);
            acc[session.instructorName].sort((a, b) => new Date(a.startsAtUtc) - new Date(b.startsAtUtc));
            return acc;
        }, {});
    }, [sessions]);

    return (
      <div className="max-w-5xl mx-auto px-4 py-8">
        <button onClick={() => navigateTo('home')} className={`mb-6 ${theme.textSub} hover:${theme.textMain} flex items-center`}>
            <ChevronRight className="rotate-180 mr-1" size={16}/> Geri Dön
        </button>
        <div className="flex flex-col md:flex-row gap-8 mb-8">
          <div className="w-full md:w-1/3 space-y-6">
            <div className={`${theme.cardBg} p-6 rounded-2xl`}>
              <img 
                src={selectedSport?.image} 
                alt={selectedSport?.title} 
                onError={(e) => handleImageError(e, selectedSport?.title)}
                className="w-full h-48 object-cover rounded-xl mb-4 opacity-90" 
              />
              <h2 className={`text-2xl font-bold ${theme.textMain}`}>{selectedSport?.title}</h2>
            </div>
            <div className={`${theme.cardBg} p-6 rounded-2xl`}>
              <label className={`block text-sm font-medium ${theme.textMain} mb-3 flex items-center`}><Calendar size={16} className="mr-2"/> Tarih Seç</label>
              <input type="date" value={date} onChange={(e) => setDate(e.target.value)} className={`w-full bg-zinc-950 border border-zinc-700 ${theme.textMain} rounded-lg p-3 focus:outline-none focus:border-emerald-500`} style={{ colorScheme: 'dark' }} />
            </div>
            {!user && <div className="bg-blue-900/20 border border-blue-800 p-4 rounded-xl text-sm text-blue-200"><p className="flex items-center font-bold mb-1"><AlertCircle size={16} className="mr-2"/> Misafir Modu</p>Şu an dersleri misafir olarak görüntülüyorsunuz. Satın almak için giriş yapmalısınız.</div>}
          </div>
          <div className="w-full md:w-2/3">
             <h2 className={`text-xl font-bold ${theme.textMain} border-b border-zinc-800 pb-2 mb-4`}>{fetching ? 'Yükleniyor...' : 'Müsait Eğitmenler & Seanslar'}</h2>
             {sessions.length === 0 && !fetching && <div className={`${theme.cardBg} p-8 text-center rounded-xl`}><p className={theme.textSub}>Bu tarihte planlanmış ders bulunamadı.</p></div>}
             <div className="space-y-6">
               {Object.entries(groupedSessions).map(([instructorName, instructorSessions]) => (
                 <div key={instructorName} className={`${theme.cardBg} p-6 rounded-2xl flex flex-col md:flex-row gap-6 transition-colors duration-300`}>
                    <div className="flex-shrink-0 text-center md:text-left md:w-1/4 flex flex-col items-center md:items-start">
                        <img src={getInstructorImage(instructorName)} alt={instructorName} className={`w-20 h-20 rounded-full border-2 mb-2 ${theme.accent === 'text-fuchsia-400' ? 'border-fuchsia-500' : 'border-emerald-500'}`} />
                        <h3 className={`font-bold text-lg ${theme.textMain}`}>{instructorName}</h3>
                        <p className={`text-xs ${theme.textSub} mt-1`}>Fitness Eğitmeni</p>
                    </div>
                    <div className="flex-grow">
                        <p className={`text-sm ${theme.textSub} mb-3`}>Müsait Saatler:</p>
                        <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                            {instructorSessions.map(session => {
                                const timeStr = new Date(session.startsAtUtc).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
                                const isFull = session.reservedCount >= session.capacity;
                                return (
                                    <button key={session.sessionId} onClick={() => handleReserve(session.sessionId, session.price.finalPrice, session.instructorName, date, timeStr)} disabled={isFull}
                                        className={`relative group p-3 rounded-lg border transition-all duration-200 flex flex-col items-center justify-center text-center ${isFull ? 'border-zinc-800 bg-zinc-900/50 text-zinc-600 cursor-not-allowed' : `${theme.buttonOutline} bg-transparent hover:bg-opacity-10`}`}>
                                        <span className="text-lg font-bold">{timeStr}</span>
                                        <span className={`text-xs mt-1 font-medium ${isFull ? 'text-zinc-600' : theme.textMain}`}>{session.price.finalPrice} ₺</span>
                                        {session.isPeak && <span className="absolute -top-2 -right-2 bg-orange-600 text-white text-[9px] px-1.5 py-0.5 rounded-full flex items-center shadow-sm"><Flame size={8} className="mr-0.5"/> PEAK</span>}
                                        <div className="mt-2 w-full bg-zinc-800 h-1 rounded-full overflow-hidden">
                                            <div className={`h-full ${isFull ? 'bg-zinc-600' : (session.isPeak ? 'bg-orange-500' : 'bg-emerald-500')}`} style={{ width: `${(session.reservedCount / session.capacity) * 100}%` }} />
                                        </div>
                                        <span className="text-[10px] mt-1 opacity-60">{isFull ? 'DOLU' : `${session.reservedCount}/${session.capacity}`}</span>
                                    </button>
                                );
                            })}
                        </div>
                    </div>
                 </div>
               ))}
             </div>
          </div>
        </div>
      </div>
    );
  };

  const SignupPage = () => {
    const [isRegister, setIsRegister] = useState(false);
    const [formData, setFormData] = useState({ username: '', password: '', coupon: '' });
    const [detectedType, setDetectedType] = useState('Standard');

    // UI İçin Anlık Kontrol (Sadece görsel geri bildirim)
    useEffect(() => {
       const c = formData.coupon?.trim().toUpperCase(); // Basit kontrol
       if (c === 'UTK-VIP' || c === 'UTK-VıP') setDetectedType('UTK-VIP');
       else if (c === 'STUDENT' || c?.startsWith('STU-')) setDetectedType('Student');
       else if (c === 'PRO' || c?.startsWith('PRM-')) setDetectedType('Premium');
       else setDetectedType('Standard');
    }, [formData.coupon]);

    const handleSubmit = (e) => {
      e.preventDefault();
      if (isRegister) {
        handleRegister({
          username: formData.username,
          password: formData.password,
          membershipType: detectedType, 
          membershipCode: formData.coupon 
        });
      } else {
        handleLogin(formData.username, formData.password);
      }
    };

    return (
      <div className="min-h-screen flex items-center justify-center py-12 px-4">
        <div className={`max-w-md w-full p-8 rounded-3xl border ${theme.cardBg}`}>
           <h2 className={`text-3xl font-black text-center mb-8 ${theme.textMain}`}>{isRegister ? 'Kayıt Ol' : 'Giriş Yap'}</h2>
           <form onSubmit={handleSubmit} className="space-y-4">
             <input required placeholder="Kullanıcı Adı" className="w-full p-3 bg-black/30 border border-zinc-700 rounded-lg text-white" value={formData.username} onChange={e => setFormData({...formData, username: e.target.value})} />
             <input required type="password" placeholder="Şifre" className="w-full p-3 bg-black/30 border border-zinc-700 rounded-lg text-white" value={formData.password} onChange={e => setFormData({...formData, password: e.target.value})} />
             {isRegister && (
               <div className="space-y-2">
                 <input 
                    placeholder="Kupon Kodu (PRM-2025)" 
                    className="w-full p-3 bg-black/30 border border-dashed border-zinc-600 rounded-lg text-white uppercase" 
                    value={formData.coupon} 
                    onChange={e => setFormData({...formData, coupon: e.target.value})} 
                 />
                 <div className={`text-xs text-center p-2 rounded 
                    ${detectedType === 'UTK-VIP' ? 'bg-gradient-to-r from-yellow-700 to-amber-600 text-yellow-100 font-bold border border-yellow-400' : 
                      detectedType === 'Premium' ? 'bg-purple-900 text-purple-200' : 
                      detectedType === 'Student' ? 'bg-blue-900 text-blue-200' : 'bg-zinc-800 text-zinc-400'}`}>
                   Paket: {detectedType === 'UTK-VIP' ? '💎 VIP LIMITED 💎' : detectedType}
                 </div>
                 {detectedType === 'Student' && <p className="text-[10px] text-blue-400 text-center">Öğrenci indirimi aktif!</p>}
               </div>
             )}
             <button type="submit" disabled={loading} className={`w-full py-4 rounded-lg font-bold text-white ${theme.button}`}>{loading ? 'İşleniyor...' : (isRegister ? 'KAYIT OL' : 'GİRİŞ YAP')}</button>
           </form>
           <button onClick={() => setIsRegister(!isRegister)} className="w-full mt-4 text-sm text-center text-zinc-500 hover:text-white">{isRegister ? 'Zaten hesabın var mı? Giriş Yap' : 'Hesabın yok mu? Kayıt Ol'}</button>
        </div>
      </div>
    );
  };

  const CartPage = () => (
    <div className="max-w-3xl mx-auto px-4 py-12">
      <h2 className={`text-3xl font-bold ${theme.textMain} mb-8`}>Sepet</h2>
      {cart.length === 0 ? (
        <div className={`text-center py-16 ${theme.cardBg} border-dashed rounded-2xl`}><p className={theme.textSub}>Sepetin boş.</p></div>
      ) : (
        <div className={theme.cardBg + " rounded-2xl overflow-hidden"}>
           {cart.map(item => (
             <div key={item.cartId} className="p-6 border-b border-zinc-800 flex justify-between items-center">
                <div><h3 className={theme.textMain}>{item.sportName} - {item.instructorName}</h3><p className={theme.textSub}>{item.date} | {item.time}</p></div>
                <div className={`text-xl font-bold ${theme.accent}`}>{item.totalPrice} ₺</div>
             </div>
           ))}
           <div className="p-6 text-right">
             <button onClick={handlePurchase} disabled={loading} className={`${theme.button} px-8 py-3 rounded-lg font-bold`}>{loading ? 'İşlem Sürüyor...' : (user ? 'ÖDEMEYİ TAMAMLA' : 'GİRİŞ YAP & SATIN AL')}</button>
             {!user && <p className="text-xs text-gray-500 mt-2">Satın alma adımında üyelik girişi gereklidir.</p>}
           </div>
        </div>
      )}
    </div>
  );

  const ProfilePage = () => (
    <div className="max-w-xl mx-auto py-12 px-4">
      <div className={`${theme.cardBg} p-8 rounded-2xl`}>
        <div className="flex items-center gap-4 mb-8">
          <div className={`p-4 rounded-full ${theme.accentBg}`}>
             {userType === 'UTK-VIP' ? <Trophy size={40} className="text-white drop-shadow-[0_0_10px_rgba(255,255,255,0.8)]" /> : <User size={32} className="text-white"/>}
          </div>
          <div><h2 className={`text-2xl font-bold ${theme.textMain}`}>{user?.username}</h2><p className={`${theme.accent} font-bold uppercase`}>{userType === 'UTK-VIP' ? '💎 VIP MEMBER 💎' : `${userType} Üye`}</p></div>
        </div>
        <button onClick={handleLogout} className="w-full py-3 border border-red-900 text-red-500 rounded-lg hover:bg-red-900/20">Çıkış Yap</button>
      </div>
    </div>
  );

  return (
    <div className={`min-h-screen ${theme.bg} font-sans transition-colors duration-700`}>
      <Navbar />
      {notification && <div className={`fixed top-24 right-5 z-50 px-6 py-4 rounded-xl shadow-2xl text-white animate-bounce flex items-center border ${notification.type === 'error' ? 'bg-red-900/80 border-red-500' : 'bg-emerald-900/80 border-emerald-500'}`}>{notification.type === 'error' ? <AlertCircle className="mr-3" /> : <CheckCircle className="mr-3" />}<span className="font-bold">{notification.msg}</span></div>}
      <main className="pb-12 pt-6">
        {currentPage === 'home' && <HomePage />}
        {currentPage === 'classes' && <ClassesPage />}
        {currentPage === 'cart' && <CartPage />}
        {currentPage === 'signup' && <SignupPage />}
        {currentPage === 'profile' && <ProfilePage />}
      </main>
    </div>
  );
}