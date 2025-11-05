import { useNavigate, useLocation } from 'react-router-dom';
import { useAppDispatch } from '../store/hooks';
import { setSelectedCategory, EventCategory } from '../store/slices/appSlice';
import svgPaths from "../imports/svg-n6pltu4jyi";

export default function BottomNavigation() {
  const navigate = useNavigate();
  const location = useLocation();
  const dispatch = useAppDispatch();

  const isScheduleView = location.pathname === '/schedule';
  
  // Determine which category is active based on the route
  const activeCategory = location.pathname.startsWith('/signup/') 
    ? location.pathname.split('/')[2] as EventCategory
    : null;

  const handleCategoryClick = (category: EventCategory) => {
    dispatch(setSelectedCategory(category));
    navigate(`/signup/${category}`);
  };

  const handleProfileClick = () => {
    if (isScheduleView) {
      // Go back to signup with the last selected category or default to vocal
      navigate('/signup/vocal');
    } else {
      navigate('/schedule');
    }
  };

  return (
    <nav className="fixed bottom-0 left-0 right-0 h-[61px] bg-[#d9d9d9] flex items-center justify-around md:justify-center md:gap-8 border-t border-[#bbb]">
      {/* Instrumental */}
      <button 
        onClick={() => handleCategoryClick('instrumental')}
        className={`flex flex-col items-center gap-1 ${activeCategory === 'instrumental' ? 'opacity-100' : 'opacity-60'}`}
      >
        <div className="w-[32px] h-[32px]">
          <svg className="block size-full" fill="none" viewBox="0 0 32 32">
            <path d={svgPaths.pa8b1080} fill="#002244" />
          </svg>
        </div>
        <span className="text-[11px] font-['Playfair_Display',_serif] font-extrabold text-[#002244]">Instrumental</span>
      </button>

      {/* Vocal */}
      <button 
        onClick={() => handleCategoryClick('vocal')}
        className={`flex flex-col items-center gap-1 ${activeCategory === 'vocal' ? 'opacity-100' : 'opacity-60'}`}
      >
        <div className="w-[29px] h-[31px]">
          <svg className="block size-full" fill="none" viewBox="0 0 29 31">
            <path d={svgPaths.p1c8be400} fill="#002244" />
            <path d={svgPaths.p301e5c00} fill="#002244" />
          </svg>
        </div>
        <span className="text-[11px] font-['Playfair_Display',_serif] font-extrabold text-[#002244]">Vocal</span>
      </button>

      {/* Account/Profile - Switch to Schedule View */}
      <button 
        onClick={handleProfileClick}
        className="flex flex-col items-center"
      >
        <div className={`w-[60px] h-[60px] -mt-[16px] transition-opacity ${isScheduleView ? 'opacity-100' : 'opacity-80'}`}>
          <svg className="block size-full" fill="none" viewBox="0 0 60 60">
            <path d={svgPaths.p905e980} fill="#002244" />
          </svg>
        </div>
      </button>

      {/* Speech */}
      <button 
        onClick={() => handleCategoryClick('speech')}
        className={`flex flex-col items-center gap-1 ${activeCategory === 'speech' ? 'opacity-100' : 'opacity-60'}`}
      >
        <div className="w-[33px] h-[33px]">
          <svg className="block size-full" fill="none" viewBox="0 0 33 33">
            <path d="M16.5 30.25V26.125" stroke="#222222" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
            <path d={svgPaths.p16616200} fill="#002244" />
          </svg>
        </div>
        <span className="text-[11px] font-['Playfair_Display',_serif] font-extrabold text-[#002244]">Speech</span>
      </button>

      {/* Creative Ministries */}
      <button 
        onClick={() => handleCategoryClick('creative')}
        className={`flex flex-col items-center gap-1 ${activeCategory === 'creative' ? 'opacity-100' : 'opacity-60'}`}
      >
        <div className="w-[24px] h-[24px]">
          <svg className="block size-full" fill="none" viewBox="0 0 24 24">
            <g transform="translate(2, 2)">
              <svg viewBox="0 0 23 22">
                <path d={svgPaths.p3ab5a300} stroke="#002244" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
                <path d={svgPaths.p1902b80} stroke="#002244" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
                <path d={svgPaths.p1d1869c0} stroke="#002244" strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" />
              </svg>
            </g>
          </svg>
        </div>
        <span className="text-[11px] font-['Playfair_Display',_serif] font-extrabold text-[#002244] text-center leading-tight">
          Creative<br/>Ministries
        </span>
      </button>
    </nav>
  );
}
