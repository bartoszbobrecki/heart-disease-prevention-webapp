-- PostgreSQL schema for cardiovascular prevention app

CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email TEXT NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  name TEXT,
  age INTEGER,
  sex TEXT,
  cholesterol INTEGER,
  smoker BOOLEAN,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);

CREATE TABLE measurements (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  timestamp TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT now(),
  heart_rate INTEGER,
  systolic INTEGER,
  diastolic INTEGER,
  activity_minutes INTEGER,
  notes TEXT
);

CREATE INDEX idx_measurements_user_time ON measurements(user_id, timestamp DESC);
