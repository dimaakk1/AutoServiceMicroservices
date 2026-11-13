import { useEffect, useRef, useState } from "react";
import { ArrowDown, ArrowLeft, ArrowRight, Gauge, ShieldCheck, Wrench } from "lucide-react";
import { Link } from "react-router-dom";
import "./DriftDemo.css";

const clamp = (value: number) => Math.min(1, Math.max(0, value));

const revealRange = (
  progress: number,
  start: number,
  fadeInEnd: number,
  fadeOutStart: number,
  end: number,
) => {
  if (progress < start || progress > end) return 0;
  if (progress < fadeInEnd) return clamp((progress - start) / (fadeInEnd - start));
  if (progress > fadeOutStart) return clamp((end - progress) / (end - fadeOutStart));
  return 1;
};

type DriftScrollSectionProps = {
  standalone?: boolean;
};

export function DriftScrollSection({ standalone = false }: DriftScrollSectionProps) {
  const sectionRef = useRef<HTMLElement>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const measureFrameRef = useRef<number | null>(null);
  const animationFrameRef = useRef<number | null>(null);
  const targetProgressRef = useRef(0);
  const videoProgressRef = useRef(0);
  const lastProgressRef = useRef(-1);
  const [progress, setProgress] = useState(0);
  const [videoReady, setVideoReady] = useState(false);

  useEffect(() => {
    window.scrollTo({ top: 0, behavior: "auto" });

    const reducedMotionQuery = window.matchMedia("(prefers-reduced-motion: reduce)");

    let lastAnimationTime: number | null = null;
    let lastSeekTime = 0;
    let initialSeekPending = true;
    const initialFrameTime = 1 / 30;

    const animateVideo = (timestamp: number) => {
      const video = videoRef.current;
      const target = targetProgressRef.current;
      const elapsed = lastAnimationTime === null
        ? 16
        : Math.min(timestamp - lastAnimationTime, 64);
      lastAnimationTime = timestamp;

      const smoothing = 1 - Math.exp(-elapsed / 85);
      let current = videoProgressRef.current + (target - videoProgressRef.current) * smoothing;
      if (Math.abs(target - current) < 0.0005) current = target;
      videoProgressRef.current = current;

      if (Math.abs(current - lastProgressRef.current) > 0.001) {
        lastProgressRef.current = current;
        setProgress(current);
      }

      if (
        video &&
        !reducedMotionQuery.matches &&
        !video.seeking &&
        timestamp - lastSeekTime >= 32 &&
        video.readyState >= HTMLMediaElement.HAVE_METADATA &&
        Number.isFinite(video.duration)
      ) {
        const usableDuration = Math.max(video.duration - 0.04 - initialFrameTime, 0);
        const targetTime = initialFrameTime + current * usableDuration;
        if (Math.abs(video.currentTime - targetTime) > 0.025) {
          video.currentTime = targetTime;
          lastSeekTime = timestamp;
        }
      }

      if (current !== target || video?.seeking) {
        animationFrameRef.current = window.requestAnimationFrame(animateVideo);
      } else {
        animationFrameRef.current = null;
        lastAnimationTime = null;
      }
    };

    const startAnimation = () => {
      if (animationFrameRef.current === null) {
        animationFrameRef.current = window.requestAnimationFrame(animateVideo);
      }
    };

    const updateFromScroll = () => {
      measureFrameRef.current = null;
      const section = sectionRef.current;
      if (!section) return;

      const rect = section.getBoundingClientRect();
      const scrollableDistance = Math.max(section.offsetHeight - window.innerHeight, 1);
      const nextProgress = clamp(-rect.top / scrollableDistance);
      targetProgressRef.current = nextProgress;
      startAnimation();
    };

    const scheduleUpdate = () => {
      if (measureFrameRef.current === null) {
        measureFrameRef.current = window.requestAnimationFrame(updateFromScroll);
      }
    };

    const video = videoRef.current;
    const handleMetadata = () => {
      video?.pause();
      if (video && Number.isFinite(video.duration)) {
        video.currentTime = Math.min(initialFrameTime, Math.max(video.duration - 0.04, 0));
      }
      scheduleUpdate();
    };

    const drawCurrentFrame = () => {
      const canvas = canvasRef.current;
      if (!video || !canvas || video.videoWidth === 0 || video.videoHeight === 0) return false;

      if (canvas.width !== video.videoWidth || canvas.height !== video.videoHeight) {
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
      }

      const context = canvas.getContext("2d", { alpha: false });
      if (!context) return false;

      context.imageSmoothingEnabled = true;
      context.imageSmoothingQuality = "high";
      context.drawImage(video, 0, 0, canvas.width, canvas.height);
      return true;
    };

    const handleSeeked = () => {
      if (!drawCurrentFrame()) return;

      if (initialSeekPending) {
        initialSeekPending = false;
        setVideoReady(true);
      }
      scheduleUpdate();
    };

    window.addEventListener("scroll", scheduleUpdate, { passive: true });
    window.addEventListener("resize", scheduleUpdate);
    reducedMotionQuery.addEventListener("change", scheduleUpdate);
    video?.addEventListener("loadedmetadata", handleMetadata);
    video?.addEventListener("seeked", handleSeeked);
    scheduleUpdate();

    return () => {
      window.removeEventListener("scroll", scheduleUpdate);
      window.removeEventListener("resize", scheduleUpdate);
      reducedMotionQuery.removeEventListener("change", scheduleUpdate);
      video?.removeEventListener("loadedmetadata", handleMetadata);
      video?.removeEventListener("seeked", handleSeeked);
      if (measureFrameRef.current !== null) window.cancelAnimationFrame(measureFrameRef.current);
      if (animationFrameRef.current !== null) window.cancelAnimationFrame(animationFrameRef.current);
    };
  }, []);

  const percent = Math.round(progress * 100);
  const introOpacity = clamp(1 - progress / 0.22);
  const diagnosticsOpacity = revealRange(progress, 0.18, 0.27, 0.42, 0.52);
  const controlOpacity = revealRange(progress, 0.46, 0.56, 0.7, 0.8);
  const finishOpacity = clamp((progress - 0.76) / 0.14);

  return (
    <div className="drift-demo">
      <section ref={sectionRef} className="drift-scroll-section" aria-label="Демонстрація scroll-анімації">
        <div className="drift-stage">
          <video
            ref={videoRef}
            className="drift-video-source"
            preload="auto"
            muted
            playsInline
            aria-hidden="true"
          >
            <source media="(max-width: 720px)" src="/videos/drift-demo-mobile.mp4" type="video/mp4" />
            <source src="/videos/drift-demo-desktop.mp4" type="video/mp4" />
          </video>
          <div className={`drift-video-poster${videoReady ? " drift-video-poster-hidden" : ""}`} aria-hidden="true" />
          <canvas
            ref={canvasRef}
            className={`drift-video${videoReady ? " drift-video-ready" : ""}`}
            aria-hidden="true"
          />

          <div className="drift-color-grade" />
          <div className="drift-vignette" />
          <div className="drift-scanlines" />

          <div className="drift-topbar">
            {standalone ? (
              <Link to="/" className="drift-back-link">
                <ArrowLeft /> Повернутися
              </Link>
            ) : (
              <div className="drift-test-label">
                <span className="drift-live-dot" /> АвтоПро / Driver experience
              </div>
            )}
            <div className="drift-test-label">
              <span className="drift-live-dot" /> {standalone ? "Test mode / Scroll film 01" : "Scroll controlled / Film 01"}
            </div>
          </div>

          <div className="drift-hud drift-hud-left" aria-hidden="true">
            <span>TRC / ON</span>
            <span>ROAD / ICE</span>
            <span>CAM / CHASE</span>
          </div>

          <div className="drift-progress" aria-label={`Прогрес анімації ${percent}%`}>
            <span>{percent.toString().padStart(2, "0")}</span>
            <div className="drift-progress-track">
              <div className="drift-progress-fill" style={{ height: `${percent}%` }} />
            </div>
            <small>100</small>
          </div>

          <article
            className="drift-scene drift-scene-intro"
            style={{ opacity: introOpacity, transform: `translate3d(0, -50%, 0) translate3d(0, ${progress * -70}px, 0)` }}
          >
            <div className="drift-kicker"><Gauge /> Scroll controlled</div>
            <h1>Контроль у кожному <span>повороті.</span></h1>
            <p>Прокручуйте сторінку — рух автомобіля точно слідує за вами.</p>
          </article>

          <article
            className="drift-scene drift-scene-side"
            style={{ opacity: diagnosticsOpacity, transform: `translate3d(0, -50%, 0) translate3d(${(0.34 - progress) * 160}px, 0, 0)` }}
          >
            <div className="drift-index">01</div>
            <div className="drift-kicker"><Wrench /> Точна діагностика</div>
            <h2>Знаходимо причину.<br />Не вгадуємо.</h2>
            <p>Повна перевірка автомобіля перед початком будь-яких робіт.</p>
          </article>

          <article
            className="drift-scene drift-scene-right"
            style={{ opacity: controlOpacity, transform: `translate3d(0, -50%, 0) translate3d(${(progress - 0.62) * 140}px, 0, 0)` }}
          >
            <div className="drift-index">02</div>
            <div className="drift-kicker"><ShieldCheck /> Прозорий процес</div>
            <h2>Ви контролюєте<br />кожен етап.</h2>
            <p>Статус ремонту, погодження вартості та історія робіт — в одному кабінеті.</p>
          </article>

          <article
            className="drift-scene drift-scene-finish"
            style={{ opacity: finishOpacity, transform: `translate3d(-50%, -50%, 0) translate3d(0, ${(1 - finishOpacity) * 55}px, 0)` }}
          >
            <div className="drift-kicker">АвтоПро / Service station</div>
            <h2>Готові до наступного<br /><span>повороту?</span></h2>
            <div className="drift-scene-actions">
              <Link to="/booking" className="drift-primary-link">Записатися <ArrowRight /></Link>
              <Link to="/services" className="drift-secondary-link">Переглянути послуги</Link>
            </div>
          </article>

          {progress < 0.08 && (
            <div className="drift-scroll-hint">
              <ArrowDown /> Листайте вниз
            </div>
          )}
        </div>
      </section>

      {standalone && (
        <section className="drift-afterword">
          <div>
            <span>Кінець тестової сцени</span>
            <h2>Сцена працює і на головній сторінці.</h2>
            <p>Окремий маршрут залишено як технічний стенд для перевірки анімації та продуктивності.</p>
          </div>
          <Link to="/" className="drift-primary-link">На головну <ArrowRight /></Link>
        </section>
      )}
    </div>
  );
}

export default function DriftDemo() {
  return <DriftScrollSection standalone />;
}
