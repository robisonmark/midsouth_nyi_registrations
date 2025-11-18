import svgPaths from "../imports/svg-n6pltu4jyi";
import { useAppSelector } from '../store/hooks';

export default function Header() {  
    const headerSubText = useAppSelector((state) => state.app.headerSubText);
    
    return (
      <>
        {/* Header - Full Width */}
        <header className="relative h-[157px] md:h-[275px] overflow-hidden w-full" style={{ clipPath: 'url(#header-clip-schedule)' }}>
            <svg className="header-path">
            <defs>
                <clipPath id="header-clip-schedule" clipPathUnits="userSpaceOnUse">
                  <path d="M95.5246 106.355C22.3478 106.355 0.684548 140.118 -1 157V0H374.991C374.654 3.37634 374.182 20.2581 374.991 60.7742C375.8 101.29 321.422 108.043 294.133 106.355H95.5246Z" />
                </clipPath>
            </defs>
            </svg>
            
            {/* <div className="inset-0 bg-[#002A48]" /> */}
            
            {/* Header Content Container */}
            <div className="max-w-[1400px] h-full">
            {/* Momentum Logo */}
            <div className="relative logo-block w-[158px] h-[35px]">
                <svg className="block size-full" fill="none" preserveAspectRatio="none" viewBox="0 0 158 35">
                <path clipRule="evenodd" d={svgPaths.p3f704780} fill="#39FF14" fillRule="evenodd" />
                </svg>
            </div>
            
            {/* Taglines */}
            {/* <p className="absolute left-[38px] md:left-[80px] top-[44px] md:top-[60px] text-[#fdfdf1] text-[11px] md:text-[13px] font-['Over_the_Rainbow',_cursive]">From God</p>
            <p className="absolute left-[142px] md:left-[230px] top-[44px] md:top-[60px] text-[#fdfdf1] text-[11px] md:text-[13px] font-['Over_the_Rainbow',_cursive] text-right translate-x-[-100%]">For God</p> */}
            
            {/* Title */}
            <h1 className="category-header leading-none relative text-[#fdfdf1] text-[24px] md:text-[36px] font-['Playfair_Display',_serif]" style={{ fontVariant: 'small-caps' }}>
                {headerSubText}
            </h1>
            </div>
        </header>
      </>
    )
}